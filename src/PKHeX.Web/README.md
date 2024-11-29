# PKHeX.Web

A proof-of-concept with Blazor WASM

## Debug

The best way to run so far is by running:

```bash
# build/watch the js project at src/PKHeX.Web/_js
cd _js
npm run build:watch
```

```bash
# run the dotnet Blazor project at the root
dotnet watch run --project ./ --no-hot-reload
```