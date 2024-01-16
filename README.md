# Binary file reader

  - [Intro](#intro)
  - [Context](#context)
    - [Binary -> decimal, hexadecimal -> ASCII](#binary-decimal-hexadecimal-ascii)
  - [Access the file directory](#access-the-file-directory)
  - [Set up the extension dictionary](#set-up-the-extension-dictionary)
  - [Read the byte data of the files](#read-the-byte-data-of-the-files)
  - [Check the output against the dictionary](#check-the-output-against-the-dictionary)
  - [Amend the extension if conflicting](#amend-the-extension-if-conflicting)
  - [Conclusion](#conclusion)

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

### Binary -> decimal, hexadecimal -> ASCII

These signatures are basically binary code translated into something more understandable.

For `.pdf`,

```console
// Binary: 00100101 01010000 01000100 01000110
// Translates into decimal: 37 80 68 70
// Or hexadecimal (more human-friendly): 25 50 44 46
// Translates into ASCII: %PDF
```

## Access the file directory

Let's begin by creating a method `ReadDirectory()` to access the directory containing the files:

```csharp
// Unit test.

[TestInitialize()]
public void Init()
{
    mock = new Program();

    directory = @"your_directory_path_here";

    mock.ReadDirectory(directory);
}

[TestMethod()]
public void Folder_Should_Output_Dir_Files_When_Init()
{
    Console.WriteLine(string.Join("\n", mock.Folder));
}
```

```csharp
// Program.

public class Program
{
    public string[] Folder { get; set; }

    public void ReadDirectory(string path)
        {
            Folder = Directory.GetFiles(path);
        }
```

```console
// Output.

C:\..\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.azerazr
C:\..\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.qskdjfhk
C:\..\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.wcxvwxcv
```

The files in the folder have bizarre extensions.

They definitely won't open.

## Set up the extension dictionary

Before looking into the signature of each file, let's declare the values to check against.

```csharp
// Unit test.

[TestMethod()]
public void Dictionary_Should_Output_Iso_KeyPair_When_Init()
{
    foreach (var entry in mock.Signatures)
    {
        foreach (byte b in entry.Key)
        {
            Console.Write($"{b} ");
        }

        Console.WriteLine($"{entry.Value}");
    }
}
```

```csharp
// Program.

public string[] Folder { get; set; }
public Dictionary<byte[], string> Signatures { get; set; }

public Program()
{
    Folder = Array.Empty<string>();

    Signatures = new Dictionary<byte[], string>(){
        {new byte[]{37, 80, 68, 70}, ".pdf"},
        {new byte[]{255, 216}, ".jpg"},
        {new byte[]{137, 80, 78, 71, 13, 10, 26, 10}, ".png"}
    };
}
```

```console
// Output.

37 80 68 70 .pdf
255 216 .jpg
137 80 78 71 13 10 26 10 .png
```

## Read the byte data of the files

Now, let's create a method `ReadFile()` to try to read the first 10 bytes of each file:

```csharp
// Unit test.

[TestMethod()]
public void ReadFile_Should_Read_All_Files_When_Directory_Exists()
{
    if (mock.Folder != Array.Empty<string>())
    {
        foreach (var file in mock.Folder)
        {
            mock.ReadFile(file);
        }

    }
    else
    {
        Assert.Fail($"File doesn't exist: {filepath}");
    }

    foreach (var entry in mock.Output)
    {
        Console.WriteLine(string.Join(" ", entry));
    }

}
```

```csharp
// Program.
// Here, we use C#'s predefined classes FileStream and BinaryReader.

public List<byte[]> Output { get; set; }

public void ReadFile(string filepath)
{
    // FileMode = how to open the file.
    // FileAccess = what can be done after opening the file.

    List<byte> output = new List<byte>();

    FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
    BinaryReader reader = new BinaryReader(fs);

    using (reader)
    {
        for (var i = reader.BaseStream.Position; i < 10; i++)
        {
            var entry = reader.ReadByte();
            output.Add(entry);
        }
    }

    Output.Add(output.ToArray());
}
```

```console
// Output.

137 80 78 71 13 10 26 10 0 0
255 216 255 224 0 16 74 70 73 70
37 80 68 70 45 49 46 55 10 37
```

## Check the output against the dictionary

We then create method `FindExtensionFromSignature()`.

This method works by iterating over each byte array in the dictionary (`.pdf`, `.jpg` and `.png`).

We compare the first byte of the array (for `.pdf` if would be `37`), against each byte in the output.

If there is a match, we check against the second byte in the sequence (`80`) and so on.

If the loop iterates over the whole sequence, we break from the loop and assign the value for the key (`.pdf`) to the `Extension` property.

```csharp
// Unit test.

[TestMethod()]
public void FindExtension_Should_Output_All_File_Extensions_In_Signature()
{
    if (mock.Folder != Array.Empty<string>())
    {
        for (int i = 0; i < mock.Folder.Length; i++)
        {
            mock.ReadFile(mock.Folder[i]);
            mock.FindExtensionFromSignature(mock.Output[i]);
            Console.WriteLine(mock.Extension);
        }
    }
    else
    {
        Assert.Fail($"File doesn't exist: {filepath}");
    }
}
```

```csharp
// Program.

public string Extension { get; set; }

public void FindExtensionFromSignature(byte[] input)
{
    foreach (var entry in Signatures)
    {
        Extension = string.Empty;

        byte expected = entry.Key[0];

        for (int i = 0; i < input.Length; i++)
        {
            byte actual = input[i];

            if (actual == expected)
            {
                for (int j = 1; j < entry.Key.Length; j++)
                {
                    byte next = input[i + j];

                    if (next != entry.Key[j])
                    {
                        break;
                    }
                }

                Extension = entry.Value;
                break;
            }
        }

        if (Extension.Length != 0)
        {
            break;
        }
    }
}
```

## Amend the extension if conflicting

We end the exercise by creating the `AmendExtension()` method.

It recovers the actual extension of the file (e.g. `.azerazr`), checks against the extension stored in the `Extension` property and amends if conflicting:

```csharp
// Unit test.

[TestMethod()]
public void AmendExtension_Should_Output_True_When_Different()
{
    if (mock.Folder != Array.Empty<string>())
    {
        for (int i = 0; i < mock.Folder.Length; i++)
        {
            mock.ReadFile(mock.Folder[i]);
            mock.FindExtensionFromSignature(mock.Output[i]);
            mock.AmendExtension(mock.Folder[i]);
        }
    }
    else
    {
        Assert.Fail($"File doesn't exist: {filepath}");
    }
}
```

```csharp
// Program.
// Here, we rely on the methods of C#'s classes Path and File.

public void AmendExtension(string filepath)
{
    string actual = Path.GetExtension(filepath);

    if (actual != Extension)
    {
        string output = Path.ChangeExtension(filepath, Extension);
        File.Move(filepath, output);
        Console.WriteLine($"File amended: {output}");
    }
    else
    {
        Console.WriteLine("Coherent file extension.");
    }
}
```

```console
// Output.

File amended: C:\..\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.png
File amended: C:\..\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.jpg
File amended: C:\..\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.pdf
```

Our files are now sanitized!

## Conclusion

Through this exercise, we've been able to:

| Description |	Skill |
| --- | --- |
| Explore the concept of file signatures. | Synthesis |
| Code a simple program to apply the concept. | Application |
| Break up the program into testable methods. | Test-driven |