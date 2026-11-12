# Local Cisco Phone AI Hotline

## Summary

Run the entire hotline on the Windows desktop: phone call handling, speech recognition, language model, and speech synthesis. The Cisco phone connects over the home network. Conversations stay local and require the desktop to be awake.

## Implementation

- Select a phone: Identify available Cisco models and firmware. Prefer a phone with accessible Multiplatform (MPP/3PCC) SIP settings, and confirm it is approved for reuse. Obtain compatible Ethernet and power equipment.
- Benchmark local AI first: Verify the GTX 1070, NVIDIA driver, and available GPU memory. Test a microphone-to-speaker conversation using whisper.cpp for speech recognition, llama.cpp with a quantized 3–4B model, and Piper for speech synthesis. Measure response delay before integrating the phone.
- Set up calling on Windows: Run Asterisk in a Linux virtual machine on the desktop. Configure the phone to register with it over the home network, then verify a test extension and two-way audio. Configure the VM’s network so the phone can reach both SIP signaling and audio ports.
- Connect the voice pipeline: Create a bridge between Asterisk and the Windows AI services. Start with a dialed extension; enable automatic calling on handset pickup if the selected phone supports it.
- Make it usable: End the conversation when the handset is hung up. Play a short failure message if the AI service is unavailable. Keep the services on the trusted home network and store no conversation history by default.

## Test Plan

- Measure transcription, response-generation, and speech playback delays; try a smaller model if the experience is too slow.
- Verify phone registration and two-way test audio, then test a full conversation and repeated calls.
- Test hangup during a response, AI service failure, and recovery after restarting the desktop and virtual machine.

## Assumptions

- The initial assistant is a friendly conversational hotline, without a public phone number or smart-home controls.
- Natural interruption handling is a later improvement; the first version can use turn-taking.
- The phone’s exact provisioning and power requirements will be established after its model and firmware are known.