# Local AI hotline: voice prototype

This first phase runs a local, turn-taking English or French conversation on the Windows server using a microphone and speakers. No phone is required.

## Server setup

1. Install the .NET 10 SDK on the build machine. From the repository root, run `dotnet build Hotline/Hotline.csproj` and `dotnet test Hotline.Tests/Hotline.Tests.csproj`.
2. Install local builds of [whisper.cpp](https://github.com/ggml-org/whisper.cpp), [llama.cpp](https://github.com/ggml-org/llama.cpp), and [Piper](https://github.com/OHF-Voice/piper1-gpl). Download a multilingual Whisper model, an instruction-tuned multilingual GGUF model (start with a quantized 3–4B model), and English and French Piper voices with their `.onnx.json` files. Check the model licenses and the projects' setup instructions.
3. Run `nvidia-smi` on the server to check its GPU and available memory. Use compatible GPU-enabled builds where available. Start llama.cpp's server, adjusting GPU offloading to fit the server:

   ```powershell
   llama-server -m C:\models\assistant.gguf -ngl 99 --host 127.0.0.1 --port 8080
   ```

4. Copy `config.example.json` to `config.json` and set the executable and model paths. Set `whisper_gpu` to `false` for a CPU-only build.
5. Run `dotnet run --project Hotline/Hotline.csproj -- --config config.json`. Choose `en` or `fr`, press Enter to start recording and Enter again to stop. Enter `q` to quit. Headphones help prevent feedback.

To deploy without installing .NET on a 64-bit Windows server, build with `dotnet publish Hotline/Hotline.csproj -c Release -r win-x64 --self-contained true` and copy the publish directory to the server. Run `Hotline.exe --config C:\path\to\config.json`. The AI executables, models, audio devices, and local llama server must still be available on that server.

The program reports transcription, generation, and synthesis times for each turn. Record timings for both languages on the server and try a smaller model if needed. Internet access is needed to download dependencies and models, but not for inference. Conversation context stays in memory, and temporary audio and transcription files are deleted at the end of each turn. Configure the model server's own logging to match your privacy needs. Restart it if it becomes unavailable.
