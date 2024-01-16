# Binary file reader

## Intro

Common file formats can be identified by a signature (or `magic number`).

| Format | Magic number (decimal) | ASCII |
| --- | --- | --- |
| .pdf | 37 80 68 70 | %PDF |
| .jpg | Starts with 255 216, ends with 255 217 | ÿØ, ÿÙ |
| .png | 137 80 78 71 13 10 26 10 | ‰PNG + strange characters |

This signature is embedded into the file data itself, when the file is created.

It is independent of the extension of the file.

If the extension is modified, the signature isn't, provided that the signature exists.

We can rely on these magic numbers to identify the format of a file.

## Context

Imagine that you have to perform an `ETL` operation on data containing hundreds of pdfs and images (see [ETL](https://github.com/sximenez/2024-01-jan-etl-extract-transform-load)).

For some reason, some files have conflicting extensions (e.g. `.pd` instead of `.pdf`).

This could potentially lead to display problems on a browser.

In this exercise, we'll implement a small program to : 

1. Read the signature of a file.
1. Look up the corresponding extension on a list.
1. Compare it with the actual extension of the file.
1. Amend if incoherent.

### Binary -> decimal, hexadecimal -> ASCII.

These signatures are basically binary code translated into something more understandable.

For `.pdf`,

```console
// Binary: 00100101 01010000 01000100 01000110
// Translates into decimal: 37 80 68 70
// Or hexadecimal (more human-friendly): 25 50 44 46
// Translates into ASCII: %PDF
```