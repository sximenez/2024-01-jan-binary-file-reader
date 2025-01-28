# Binary file reader

  <!--TOC-->
  - [Intro](#intro)
    - [Binary -> decimal, hexadecimal -> ASCII](#binary-decimal-hexadecimal-ascii)
  - [Practice](#practice)
    - [Class declaration](#class-declaration)
    - [Read files](#read-files)
    - [Find extensions from signatures](#find-extensions-from-signatures)
    - [Amend extensions if incoherent](#amend-extensions-if-incoherent)
    - [Main entry point](#main-entry-point)
  - [Conclusion](#conclusion)
<!--/TOC-->

## Intro

Common file formats can be identified by a signature or `magic number`:

| Format | Magic number (decimal) | ASCII |
| --- | --- | --- |
| .pdf | 37 80 68 70 | %PDF |
| .jpg | Starts with 255 216, ends with 255 217 | ÿØ, ÿÙ |
| .png | 137 80 78 71 13 10 26 10 | ‰PNG + strange characters |

This signature is embedded into the file data itself, when the file is created.

It is independent of the extension of the file.

If the extension is modified, we can rely on the signature to accurately identify the format.

### Binary -> decimal, hexadecimal -> ASCII

These signatures are basically binary code translated into something more understandable.

For `.pdf`,

```console
// Binary: 00100101 01010000 01000100 01000110
// Translates into decimal: 37 80 68 70
// Or hexadecimal (more human-friendly): 25 50 44 46
// Translates into ASCII: %PDF
```

## Practice

We have to perform an `ETL` operation on data containing hundreds of pdfs and images (see [ETL](https://github.com/sximenez/2024-01-jan-etl-extract-transform-load)).

For some reason, some files have conflicting extensions (e.g. `.pd` instead of `.pdf`).

This leads to display problems on the browser.

In this exercise, we'll implement a small program to: 

1. Read the signature of a file.
1. Look up the corresponding extension on a list.
1. Compare it with the actual extension of the file.
1. Amend if incoherent.

### Class declaration

```csharp
public string[] Files { get; private set; } // Stores the files to process.
public Dictionary<byte[], string> Signatures { get; } // Stores the target signatures.
```

```csharp
public Program(string dirPath)
{
    Files = Directory.GetFiles(dirPath);
    Signatures = new Dictionary<byte[], string>
    {
        { new byte[] { 37, 80, 68, 70 }, ".pdf"},
        { new byte[] { 255, 216 }, ".jpg"},
        { new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, ".png"}
    };
}
```

### Read files

```csharp
internal byte[] ReadFile(string filePath) // It takes in a file path.
{
    // FileMode = how to open the file.
    // FileAccess = what can be done after opening the file.

    byte[] sequence = new byte[10]; // We can limit our sequences to ten bytes.

    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read); // FileStream allows to access and read the file.
    BinaryReader reader = new BinaryReader(fs); // BinaryReader simplifies the reading operation.

    using (reader)
    {
        for (var i = reader.BaseStream.Position; i < 10; i++)
        {
            sequence[i] = reader.ReadByte(); // We read each byte and store it.
        }
    }

    return sequence; // We return the byte array.
}
```

```csharp
// Unit test.

private Program _mock;
private string _directory;
private string _corruptedDir;

[TestInitialize()]
public void Init()
{
    _directory = @"C:\Users\steven.jimenez\source\repos\2024-01-jan-binary-file-reader\test_files";
    _corruptedDir = @"C:\Users\steven.jimenez\source\repos\2024-01-jan-binary-file-reader\test_files\corrupted";

    _mock = new Program(_directory);
}

[TestMethod()]
public void ReadFile_ValidDir_ValidOutput()
{
    var expected = new List<byte[]>()
    {
        new byte[] { 255, 216, 255, 224, 0, 16, 74, 70, 73, 70 },
        new byte[] { 37, 80, 68, 70, 45, 49, 46, 55, 10, 37 },
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0 }
    };

    for (int i = 0; i < _mock.Files.Length; i++)
    {
        var actual = _mock.ReadFile(_mock.Files[i]);
        CollectionAssert.AreEqual(expected[i], actual);
        Console.WriteLine(string.Join(" ", actual));
    }
}
```

### Find extensions from signatures

```csharp
internal string FindExtensionFromSignature(byte[] input)
{
    foreach (var entry in Signatures) // We loop on each target signature.
    {
        byte[] signature = entry.Key;

        // First check: the input signature needs to be at least as long as the target signature.
        if (input.Length >= signature.Length)
        {
            // Second check: if the input signature is equal to the target signature, return the target extension.
            if (input.Take(signature.Length).SequenceEqual(signature))
            {
                return entry.Value;
            }
        }
    }

    return string.Empty;
}
```

```csharp
[TestMethod()]
public void FindExtension_ValidDir_ValidOutput()
{
    var expected = new string[] {
        ".jpg",
        ".pdf",
        ".png"
    };

    for (int i = 0; i < _mock.Files.Length; i++)
    {
        var byteSequence = _mock.ReadFile(_mock.Files[i]);
        var actual = _mock.FindExtensionFromSignature(byteSequence);

        Assert.AreEqual(expected[i], actual);
    }
}
```

### Amend extensions if incoherent

```csharp
internal void AmendExtension(string filePath, string extension)
{
    string actual = Path.GetExtension(filePath); // Actual file name without processing.
    string expected = Path.ChangeExtension(filePath, extension); // Expected file name after processing.

    if (actual == extension) // If correct, return.
    {
        return;
    }

    File.Move(filePath, expected); // Otherwise, specify new file extension on name.
}
```

```csharp
[TestMethod()]
public void AmendExtension_InvalidInput_ValidOutput()
{
    var testDir = Directory.CreateDirectory(Path.Combine(_directory, "unit-test")).FullName; // Create test directory.
    var corruptedFiles = Directory.GetFiles(_corruptedDir);

    // Copy corrupted test files to created test directory.
    foreach (string file in corruptedFiles)
    {
        File.Copy(file, Path.Combine(testDir, Path.GetFileName(file)), true);
    };

    // Process corrupted files.
    var testFiles = Directory.GetFiles(testDir);
    foreach (var file in testFiles)
    {
        var byteSequence = _mock.ReadFile(file);
        var extension = _mock.FindExtensionFromSignature(byteSequence);
        _mock.AmendExtension(file, extension);
    };

    string[] expected = _mock.Files.Select(e => Path.GetFileName(e)).ToArray();
    string[] actual = Directory.GetFiles(testDir).Select(e => Path.GetFileName(e)).ToArray();

    // Assert and delete test directory.
    CollectionAssert.AreEqual(expected, actual);
    Directory.Delete(testDir, true);
}
```

### Main entry point

```csharp
public static void Main(string[] args)
{
    try
    {
        var reader = new Program(@"C:\...\2024-01-jan-binary-file-reader\test_files\corrupted");

        for (int i = 0; i < reader.Files.Length; i++)
        {
            string filePath = reader.Files[i];

            var byteSequence = reader.ReadFile(filePath);
            string extension = reader.FindExtensionFromSignature(byteSequence);
            reader.AmendExtension(filePath, extension);
        }
    }
    catch (Exception)
    {
        throw;
    }
}
```

```console
// Input.

C:\..\2024-01-jan-binary-file-reader\test_files\unit-test\jesus-curiel-1YpDkYsoggw-unsplash.azerazr
C:\..\2024-01-jan-binary-file-reader\test_files\unit-test\jesus-curiel-1YpDkYsoggw-unsplash.qskdjfhk
C:\..\2024-01-jan-binary-file-reader\test_files\unit-test\jesus-curiel-1YpDkYsoggw-unsplash.wcxvwxcv
```

```console
// Byte sequences.

137 80 78 71 13 10 26 10 0 0
255 216 255 224 0 16 74 70 73 70
37 80 68 70 45 49 46 55 10 37
```

```console
// Output.

C:\..\2024-01-jan-binary-file-reader\test_files\unit-test\jesus-curiel-1YpDkYsoggw-unsplash.png
C:\..\2024-01-jan-binary-file-reader\test_files\unit-test\jesus-curiel-1YpDkYsoggw-unsplash.jpg
C:\..\2024-01-jan-binary-file-reader\test_files\unit-test\jesus-curiel-1YpDkYsoggw-unsplash.pdf
```

Input files are sanitized as a result.

## Conclusion

Through this exercise, we've been able to:

| Description |	Skill |
| --- | --- |
| Explore the concept of file signatures. | Synthesis |
| Code a simple program to apply the concept. | Application |
| Break up the program into testable methods. | Test-driven |