# 🔎 ImageOCR

![.NET](https://img.shields.io/badge/.NET-10.0--windows-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![Tesseract](https://img.shields.io/badge/Tesseract-5.2-5C3EE8?style=flat-square)
![PDFtoImage](https://img.shields.io/badge/PDFtoImage-5.2-1f6feb?style=flat-square)
![Blog](https://img.shields.io/badge/blog-rsrsystem-FF5722?style=flat-square&logo=blogger&logoColor=white)

> Librerías **C# / .NET 10** para hacer **OCR** desde PowerBuilder: imagen → texto, PDF → imagen y captura del portapapeles.

## 📋 ¿Qué es esto?

La solución agrupa **tres librerías** que se complementan para montar un flujo de OCR completo y
consumirlas desde PowerBuilder como `dotnetobject`:

| Proyecto | Qué hace | Apoyado en |
|----------|----------|------------|
| **`ImageOCR`** | Reconoce texto de una imagen (OCR) y lo devuelve / guarda en `.txt` | [Tesseract](https://github.com/charlesw/tesseract) `5.2` (idioma `spa`) |
| **`ImageFromPdf`** | Rasteriza páginas de un PDF a **BMP/PNG** (paso previo al OCR) | **[PDFtoImage](https://github.com/sungaila/PDFtoImage) `5.2`** (PDFium + SkiaSharp) |
| **`ImageFromClipboard`** | Vuelca la imagen del **portapapeles** a un fichero | WinForms + System.Drawing |

> 🆕 **Novedad de la migración a .NET 10:** `ImageFromPdf` ya **no usa el abandonado PdfiumViewer
> (2018)**. Ahora rasteriza con **PDFtoImage** (MIT, mantenido, PDFium + SkiaSharp).

## 🧩 Dependencias

| Paquete | Versión | Proyecto |
|---------|---------|----------|
| [Tesseract](https://www.nuget.org/packages/Tesseract) | `5.2.0` | ImageOCR |
| [PDFtoImage](https://www.nuget.org/packages/PDFtoImage) | `5.2.1` | ImageFromPdf |
| [System.Drawing.Common](https://www.nuget.org/packages/System.Drawing.Common) | `10.0.9` | ImageOCR · ImageFromPdf |

## 🛠️ Requisitos

- **.NET SDK 10.0** o superior
- **Windows** (las tres librerías son `net10.0-windows`)
- Los datos de idioma de Tesseract (`tessdata`, p. ej. `spa.traineddata`) junto a la DLL

## 🚀 Compilar

```bat
dotnet build ImageOCR.sln -c Release
```

## 🔗 Proyecto PowerBuilder relacionado

👉 **pbImageOCR** — https://github.com/rasanfe/pbImageOCR

---

📨 **Blog:** <https://rsrsystem.blogspot.com/>

> ¡Nos vemos en el próximo artículo! Y recuerda: en PowerBuilder, los límites solo están en nuestra imaginación. 🚀
