# FastIR Collector Agent

## Description
FastIR Collector Agent is a Windows service. It connects to a server (like [FastIR Server](https://github.com/SekoiaLab/FastIR_Server)) to receive the order to execute the [FastIR Collector](https://github.com/SekoiaLab/Fastir_Collector).

## Installation
The project was compiled and tested on Visual Studio 2015.

Prerequired:
- download and install this extension: https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9
- put dotnetfx45_full_x86_x64.exe (available on MSDN) in the directory: C:\Program Files (x86)\Microsoft Visual Studio 14.0\SDK\Bootstrapper\Packages\

## Configuration
The configuration is stored in `HKLM\SYSTEM\CurrentControlSet\Services\FastIR`:
<pre>
APIKey -> the API key to connect to the server
Port -> the port of the server
PUBLIC_SSL -> the public key of the HTTPS server (SSL Pinning)
RefreshMin -> the frequency of connections to the server per minute
URL -> the IP/DNS of the server
</pre>
