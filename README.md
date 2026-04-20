# mp3info

`mp3info` is a .NET command-line tool for working with MP3 and FLAC music libraries.

It scans a directory recursively, reads tags, and helps you:

- write and validate per-track content hashes
- normalize problematic tags
- export embedded artwork to image files
- rename tracks into a consistent folder/file layout
- find duplicate files by hash
- export track metadata to CSV

## What the program does

The tool processes `.mp3` and `.flac` files under a root path.

Its primary workflow is `fix`, which performs these actions for each track:

1. Ensure a base64 SHA-256 content hash is present in tags (`hash` field)
2. Normalize tag issues (including ID3 cleanup/fixes)
3. Rename tracks into a structured layout
4. Export embedded art to `folder.jpg` / `folder.png` / `folder.gif` (and remove exported art from tags)
5. Remove empty directories left behind after moves

## Requirements

- .NET SDK 10.0+

## Build and test

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal /p:CollectCoverage=true
```

## Run

From the repository root:

```bash
dotnet run --project ./MP3Info -- --help
```

## Commands

```text
validate <path>   Validate hash in mp3 comments.
listdupes <path>  List duplicate mp3 files.
fix <path>        Fix names, export art, add hash.
list <path>       List mp3 metadata to csv.
```

### validate

Validate stored hashes against track content.

```bash
dotnet run --project ./MP3Info -- validate /music/library
dotnet run --project ./MP3Info -- validate /music/library --verbose
```

### listdupes

Group tracks by hash and log duplicates.

```bash
dotnet run --project ./MP3Info -- listdupes /music/library
```

### fix

Apply hash/tag/art/rename cleanup workflow.

```bash
dotnet run --project ./MP3Info -- fix /music/library
```

Options:

- `-w, --whatif` preview changes without writing files
- `-f, --force` overwrite invalid/badly formatted existing hashes

### list

Export discovered track metadata to CSV.

```bash
dotnet run --project ./MP3Info -- list /music/library
dotnet run --project ./MP3Info -- list /music/library --outfile tracks.csv
```

## Naming convention used by `fix`

When enough metadata is present, files are renamed to:

```text
<root>/<AlbumArtist>/<Album>/<Disc:00><Track:00> <Hash>.<ext>
```

Example:

```text
/music/Artist Name/Album Name/0107 qfBRTb9LheSXVPw2QBB7bgY7k4GlDjsHPl48C7jFfqU=.mp3
```

## Docker

Build image:

```bash
docker build -t mp3info .
```

Run:

```bash
docker run --rm -v /path/to/music:/music mp3info fix /music
```

## License

MIT (see `LICENSE`).
