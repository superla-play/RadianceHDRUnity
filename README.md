# Radiance HDR for Unity

[![openupm](https://img.shields.io/npm/v/tv.superla.radiancehdr?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/tv.superla.radiancehdr/)
[![GitHub issues](https://img.shields.io/github/issues/superla-play/RadianceHDRUnity)](https://github.com/superla-play/RadianceHDRUnity/issues)
[![GitHub license](https://img.shields.io/github/license/superla-play/RadianceHDRUnity)](https://github.com/superla-play/RadianceHDRUnity/blob/main/LICENSE.md)

This package enables the ability to load Radiance HDR (RGBE) images
at runtime in Unity. We support RLE and non-RLE encoded images.

## Installing
The easiest way to install is to download and open the
[Installer Package](https://package-installer.glitch.me/v1/installer/OpenUPM/tv.superla.radiancehdr?registry=https%3A%2F%2Fpackage.openupm.com&scope=tv.superla).

It runs a script that installs the package via a
[scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html).

Afterwards, Radiance HDR for Unity is listed in the*Package Manager*
(under *My Registries*) and can be installed and updated from there.

<details><summary>Alternative: Install via GIT URL</summary>
Add Radiance HDR for Unity via Unity's Package Manager
( Window -> Package Manager ).
Click the ➕ on the top left and choose *Add package from GIT URL*.

Enter the following URL:
`https://github.com/superla-play/RadianceHDRUnity.git#upm`
</details>

---
## Usage
```cs
// Make sure to include the namespace!
using Superla.RadianceHDR;

...
// Load the image however you like in the form of a byte array, be it from
// streaming assets or from the web, etc.
UnityWebRequest www = UnityWebRequest.Get("path/to/image.hdr");
www.downloadHandler = new DownloadHandlerBuffer();
var asyncOp = www.SendWebRequest();
while (!asyncOp.isDone)
{
    await Task.Yield();
}
byte[] imageData = www.downloadHandler.data;

// Pass the byte array to the constructor.
RadianceHDRTexture hdr = new RadianceHDRTexture(imageData);

// Retrieve the generated texture.
Texture2D texture = hdr.texture;
```

---
## Motivation
During the course of development for other Unity-based projects,
we regularly used HDR images for background
and lighting purposes in our scenes.
Unity handles editor-time import of HDR images without hassle.
While exploring options for user-customisable content,
we were surprised to find the engine did *not* support
runtime loading of HDR images. We decided that needed to be changed!

## Acknowlegements
Radiance HDR for Unity was inspired by the excellent
[three.js](https://threejs.org/) and it's
[RGBELoader](https://github.com/mrdoob/three.js/blob/dev/examples/jsm/loaders/RGBELoader.js),
which was in turn adapted from Bruce Walter's
[C Loader](http://www.graphics.cornell.edu/~bjw/rgbe.html) for RGBE.

## License

Copyright (c) 2022-2024 Superla.tv, All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use files in this repository except in compliance with the License.
You may obtain a copy of the License at

   <http://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

## Trademarks

*Unity* is a registered trademark of [Unity Technologies](https://unity.com).
