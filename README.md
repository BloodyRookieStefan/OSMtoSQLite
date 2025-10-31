# OSMtoSQLite

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![SQLite](https://img.shields.io/badge/SQLite-supported-green.svg)](https://www.sqlite.org/)
[![License](https://img.shields.io/github/license/BloodyRookieStefan/OSMtoSQLite)](LICENSE)

A .NET library for converting OpenStreetMap (OSM) data files to SQLite databases with support for filtering and bounding box extraction.

## 🚀 Features

- **Multiple Input Formats**: Supports both `.osm` and `.bz2` compressed OSM files
- **Flexible Filtering**: Extract specific data types (buildings, highways, railways, waterways, etc.)
- **Bounding Box Support**: Extract data within specific geographic boundaries
- **Performance Optimized**: Efficient processing of large OSM datasets
- **SQLite Output**: Generates structured SQLite databases for easy data access
- **Comprehensive API**: Multiple overloads for different use cases

## 📋 Requirements

- .NET Framework 4.7.2 or higher
- SQLite support (included via NuGet packages)
- Java Runtime Environment (JRE) - required for BZ2 decompression and filtering operations

## 🛠️ Installation

### Option 1: Build from Source

1. Clone the repository:
```bash
git clone https://github.com/BloodyRookieStefan/OSMtoSQLite.git
cd OSMtoSQLite
```

2. Open `OSMtoSQLite.sln` in Visual Studio

3. Restore NuGet packages and build the solution

### Option 2: Use the Library

Add the compiled `OSMConverter.dll` to your project references.

## 📖 Usage

### Basic Usage

```csharp
using OSMConverter;

// Convert an OSM file to SQLite (no filtering)
Convert.OSMtoSQLite("path/to/data.osm", "output/directory");

// Convert a BZ2 compressed file
Convert.BZ2toSQLite("path/to/data.osm.bz2", "output/directory");
```

### Filtering Data

```csharp
using OSMConverter;

// Extract only buildings
Convert.OSMtoSQLite("data.osm", "output", Filter.Buildings);

// Extract multiple data types
var filters = new List<Filter> 
{ 
    Filter.Buildings, 
    Filter.Highway, 
    Filter.Railway 
};
Convert.OSMtoSQLite("data.osm", "output", filters);
```

### Bounding Box Extraction

```csharp
using OSMConverter;
using System.Windows;

// Define bounding box coordinates
Point northWest = new Point(48.9850614, 9.2147826);  // lat, lon
Point southEast = new Point(48.935751, 9.3031024);

// Extract data within bounding box
var filters = new List<Filter> { Filter.Buildings, Filter.Highway };
Convert.OSMtoSQLite("data.osm.bz2", "output", filters, northWest, southEast);
```

### Complete Example

```csharp
using OSMConverter;
using System;
using System.Collections.Generic;
using System.Windows;

class Program
{
    static void Main()
    {
        try
        {
            // Define extraction area (Stuttgart region example)
            Point northWest = new Point(48.9850614, 9.2147826);
            Point southEast = new Point(48.935751, 9.3031024);
            
            // Define what data to extract
            var filters = new List<Filter> 
            { 
                Filter.Buildings, 
                Filter.Highway, 
                Filter.Railway,
                Filter.Wood,
                Filter.Waterway
            };
            
            // Perform conversion
            Convert.BZ2toSQLite(
                "stuttgart-regbez-latest.osm.bz2",
                "SQLDatabase",
                filters,
                northWest,
                southEast
            );
            
            // Display performance statistics
            Console.WriteLine($"Processing completed!");
            Console.WriteLine($"Total time: {Infos.GetOverallTime()}");
            Console.WriteLine($"Nodes processed: {Infos.GetTotalNodeCount()}");
            Console.WriteLine($"Ways processed: {Infos.GetTotalWayCount()}");
            Console.WriteLine($"Relations processed: {Infos.GetTotalRelationCount()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

## 🗂️ Available Filters

| Filter | Description |
|--------|-------------|
| `Filter.None` | No filtering (includes all data) |
| `Filter.Buildings` | Building-related data |
| `Filter.Highway` | Roads, paths, and transportation routes |
| `Filter.Railway` | Railway infrastructure |
| `Filter.Waterway` | Rivers, streams, and water features |
| `Filter.Wood` | Forests and wooded areas |

## 📊 Output Structure

The library generates SQLite databases in the specified output directory:

- `BoundingBox.sqlite` - Data within the specified bounding box (if provided)
- `[FilterName].sqlite` - Separate databases for each filter type

Each database contains tables for:
- **Nodes**: Geographic points with coordinates and tags
- **Ways**: Ordered sequences of nodes (roads, boundaries, etc.)
- **Relations**: Complex relationships between nodes and ways

## ⚡ Performance

The library includes built-in performance monitoring:

```csharp
// Get timing information
TimeSpan overallTime = Infos.GetOverallTime();
TimeSpan filterTime = Infos.GetOverallFilterTime();
TimeSpan boundingBoxTime = Infos.GetFilterTimeBoundingBox();
TimeSpan storingTime = Infos.GetStoringTime();

// Get processing statistics
long nodeCount = Infos.GetTotalNodeCount();
long wayCount = Infos.GetTotalWayCount();
long relationCount = Infos.GetTotalRelationCount();
```

## 🏗️ Project Structure

```
OSMtoSQLite/
├── OSMtoSQLite/             # Main library project
│   ├── Convert.cs           # Main conversion API
│   ├── Infos.cs             # Performance statistics
│   └── lib/                 # Core library components
│       ├── DataFilter.cs    # BZ2 decompression and filtering
│       ├── OSMReader.cs     # OSM XML parsing
│       ├── SQLController.cs # SQLite database operations
│       ├── Types.cs         # OSM data structures
│       └── ...
├── TestFramework/           # Example usage and testing
├── DebugData/               # Sample data files
└── packages/                # NuGet dependencies
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🐛 Issues and Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/BloodyRookieStefan/OSMtoSQLite/issues) page
2. Create a new issue with detailed information about your problem
3. Include sample data and error messages when possible

## 🙏 Acknowledgments

- [OpenStreetMap](https://www.openstreetmap.org/) for providing the data format and ecosystem
- [SQLite](https://www.sqlite.org/) for the database engine
- Entity Framework for ORM support

---

**Note**: Make sure you have a valid Java installation when using BZ2 files or filtering operations, as these features require Java for decompression and processing.
