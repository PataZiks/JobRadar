# Rotating the Jooble API Key

Use this guide when your current Jooble key is exhausted or compromised.

> Full implementation details are added in v3. This is the preview.

## Steps

1. **Get a new key** at [jooble.org](https://jooble.org) – log in, go to API settings.

2. **Update local secret**
   Edit `.env` (never commit this file):
   ```
   JOOBLE_API_KEY=your-new-key-here
   ```

3. **Update GitHub Secret**
   - Go to your repository on GitHub
   - Settings → Secrets and variables → Actions
   - Click the `JOOBLE_API_KEY` secret → Update

4. **Verify the new key** (costs exactly 1 API request):
   ```
   dotnet run --project src/JobRadar.Cli -- check-key
   ```
   Expected output: `Jooble key OK – 1/500 used`

5. The request counter resets automatically for the new key hash.

## Notes
- The key is stored in **exactly one place** locally (`.env`) and one place on GitHub (Secret).
- No code changes needed. No rebuild needed.
- If the old key is compromised (accidentally committed), also revoke it on jooble.org immediately.
