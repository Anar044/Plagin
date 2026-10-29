# Smart Horeca QR POS Bridge V1 — C# plugin

**Status:** staged source code, draft PR only. No installed cash-register DLL is changed. Do not enable on production tills until reviewed, built against the actual host SDK, and tested with a real POS terminal.

## Project files

- `QrPosBridgeSettings.cs` — disabled by default.
- `FileConfig.cs` — optional `QrPosBridge` node from the existing `smarthoreca.json`.
- `QrPosBridgeService.cs` — HTTPS hello/pull/ackCreated/ackFailed/ackFiscalized (fiscal acknowledgement only with real fiscal register proof).
- `HorecaControlChequeTaskProcessor.cs` — watches for successful fiscal-device callbacks, persists receipt data for background acknowledgement. No new receipt is created by this callback.
- `HorecaControl.cs` — start/dispose hook isolated from legacy Socket.IO and push notifications.

## Local configuration (example — no real secrets)

Put into the `smarthoreca.json` selected by `FileConfig.GetConfig()`, alongside the existing `PluginId`. The fallback storage copy is used only if the primary config does not contain the plugin ID. Protect the file with NTFS permissions allowing only the POS service account and administrators.

```json
{
  "PluginId": "your-registered-plugin-guid",
  "QrPosBridge": {
    "Enabled": false,
    "AllowOrderCreation": false,
    "AllowKitchenPrint": false,
    "Url": "https://smarthoreca.pages.dev/api/qr-menu/pos-bridge",
    "PluginIngressToken": "SET_BY_ADMIN_NOT_IN_GIT",
    "BridgeToken": "SET_BY_ADMIN_NOT_IN_GIT",
    "OperatorPin": "SET_ONLY_LOCALLY_ON_TEST_POS",
    "PollSeconds": 10
  }
}
```

Create a dedicated iikoFront operator with only the permissions required to open QR orders. Its personal PIN is required for SubmitChanges and must remain only in the protected POS configuration; no default PIN is provided. Never store a PIN in GitHub, logs or screenshots.

Do not share live tokens in chats, logs, screenshots, repositories or browser code. Rotate any inadvertently exposed token. `PluginIngressToken` must match the backend's `PLUGIN_INGEST_TOKEN`; `BridgeToken` must match `QR_POS_BRIDGE_TOKEN`, set only on Cloudflare/server secrets.

## Fail-safe operation

- Startup does nothing unless **all three** `Enabled`, `AllowOrderCreation` and `AllowKitchenPrint` are true and credentials + HTTPS URI have been validated. This is a safety gate: guests cannot submit QR orders until the plugin can actually send their dishes to the kitchen.
- In production `Url` must be the **public** HTTPS Cloudflare Pages deployment host with exact endpoint path `/api/qr-menu/pos-bridge`.
- Each request authenticates using BOTH secrets; backend validates `pluginId` and `departmentId` against the live plugin registry.
- Hello advertises protocol v1. Pull returns 0 or 1 order, with a 90-second lease. For a retry, **never** create an order twice:
  - Check iikoFront external data key `sh.qr.external_order_id` on existing orders.
  - Maintain a durable local PREPARED/CREATED journal in plugin storage.
  - PREPARED but no matching POS order is **POS_RESULT_UNKNOWN** requiring reconciliation, not automatic re-creation.
- Validate AZN, one payment mode `PAY_AT_POS`, restaurant, product IDs, simple Dish only, no sizes/compound positions/modifiers, POS table ID for TABLE mode, active cash session and every menu price against POS current price.
- Create an iikoFront order with the external id in **the same edit session** as its products, via `SubmitChanges`. **No online payment** and **no automatic closing**.
- After iikoFront returns a real order ID, send `ackCreated`. If network fails, the local journal can recover the same order.
- A successful cash-register fiscal operation can provide a genuine `DocumentNumber` and `DocumentSign`. The callback stores it as `FISCAL_PENDING` and the worker retries `ackFiscalized` until confirmed. Cash-only and card-only operations supported. Never infer fiscalization from simply closing the order.
- If device does not return reliable document proof, or payment is mixed, mark for human reconciliation and **never invent a receipt number or RRN**.
- Kitchen service printing uses `PrintOrderItems` for the new order only, after the real POS order is created and acknowledged. Before invoking the printer, the plugin durably marks `IN_PROGRESS`. After success it marks `PRINTED`; on exception or process interruption it marks `CHECK_REQUIRED` and **does not repeat the print automatically**. The cashier must inspect kitchen tickets and reconcile the ambiguous outcome. The backend receives `ackKitchen` and stores it separately from `POS_OPEN` and `CLOSED`.

## Important V8/V8P5 compatibility

The source `Resto.Front.Api.HorecaControlPlugin.csproj` defaults to stable `Resto.Front.Api.V8` NuGet v8.7.6032. GitHub Windows build currently uses `-p:APIVersion=V8P5` (V8Preview5 8.4.6018-alpha). Existing terminals may run a different iikoFront 8.4.x and have other supported plugin API versions. **A successful CI build does not prove runtime API compatibility.** Use the exact API version and compatible licensing on the test till.

GitHub Actions: `Plugin Official NuGet Build`; the workflow compiles only, it does not publish a new DLL to any cash register.

## Test scenario

1. Deploy the QR-order API from [anarsystem PR #214](https://github.com/Anar044/anarsystem/pull/214) to staging, configure D1, `QR_ORDER_RATE_SECRET`, `PLUGIN_INGEST_TOKEN`, `QR_POS_BRIDGE_TOKEN`.
2. Register test plugin ID and exact restaurant department in the backend registry. Configure a real iikoFront table GUID and a published menu with a basic product ID and matching live price.
3. On the **test** POS only, enable local bridge flags, restart plugin and verify `hello` is accepted. Confirm Smart Horeca admin reports the correct POS readiness.
4. Place order from a table QR: verify D1 `AWAITING_POS → LEASED → POS_OPEN` and real iikoFront `Order.Id`, item count, price and section. Verify proper waiter assignment manually.
5. Unplug network *after* POS creation but before ack. Restore; there must be only **one** POS order, retrieved by external ID.
6. Attempt wrong restaurant/plugin ID, wrong product, stale price, no cash shift, unknown table ID — must reject without a POS order.
7. Verify successful kitchen service printing, actual paper tickets and correct routing. Disconnect the printer or restart the plugin mid-print: confirm no automatic duplicate ticket; Smart Horeca must show `CHECK_REQUIRED`.
8. Perform manual POS payment on the test cash register. Verify genuine fiscal device result, check `FISCAL_PENDING → FISCAL_ACKED`, D1 `CLOSED` only when receipt proof exists; test failed receipt and mixed payment do not falsely close the QR order.
9. Check modifiers, shifts after midnight, offline tills, restaurant Chain isolation and refunds before any production release.

## Known limitations

- Modifier and combo mapping, multi-size products, direct waiter mapping, merge-with-existing-table-order and refunds still require separate development and tests. Basic kitchen printing and guest note transfer are now implemented, but have **not** been verified on a real printer. Unknown printer outcomes require human reconciliation.
- Front V8 and V8P5 build is not a substitute for a matching installed DLL check and a test on the customer's Front runtime.
- A successful fiscal callback does not provide a Payriff bank RRN. That is reserved for a later, separately verified online payment integration.
