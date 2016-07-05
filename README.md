#FastIR Collector Agent

## Description
FastIR Collector Agent is a Windows service. It connect to a server (FastIR Server) in order to receive the order to execute the FastIR Collector

## Installation
The project was compiled and test on Visual Studio 2015.

## Configuration
The configuration is stored in HKLM\SYSTEM\CurrentControlSet\Services\FastIR:
<pre>
APIKey -> the API key to connect to the server
Port -> the port of the server
PUBLIC_SSL -> the public key of the HTTPS server (SSL Pinning)
RefreshMin -> the freency of connection to the server in minute
URL -> the IP/DNS of the server
</pre>