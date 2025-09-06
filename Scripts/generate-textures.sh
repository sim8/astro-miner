#!/bin/bash

# Generates AstroMiner/Definitions/Tx.cs with string constants for all PNG files in the Content/img directory.
# Run from the project root: ./Scripts/generate-textures.sh

# Configuration
sourceDirectory="AstroMiner/Content/img"
destinationPath="AstroMiner/Definitions/Tx.cs"

# Function to convert filename to property name (remove extension)
to_property_name() {
    local filename="$1"
    echo "${filename%.*}"
}

# Function to generate class content recursively
generate_class_content() {
    local dir="$1"
    local indent="$2"
    local relative_path="$3"
    
    # Process PNG files in current directory
    for file in "$dir"/*.png; do
        [ -f "$file" ] || continue
        local filename=$(basename "$file")
        local property_name=$(to_property_name "$filename")
        local file_path="$relative_path$(to_property_name "$filename")"
        echo "${indent}public const string $property_name = \"$file_path\";"
    done
    
    # Process subdirectories
    for subdir in "$dir"/*/; do
        [ -d "$subdir" ] || continue
        local dirname=$(basename "$subdir")
        local sub_relative_path="$relative_path$dirname/"
        
        echo "${indent}public static class $dirname"
        echo "${indent}{"
        generate_class_content "$subdir" "$indent    " "$sub_relative_path"
        echo "${indent}}"
    done
}

# Function to collect all texture paths
collect_all_textures() {
    local dir="$1"
    local relative_path="$2"
    
    # Process PNG files in current directory
    for file in "$dir"/*.png; do
        [ -f "$file" ] || continue
        local filename=$(basename "$file")
        local property_name=$(to_property_name "$filename")
        local file_path="$relative_path$(to_property_name "$filename")"
        echo "            \"$file_path\","
    done
    
    # Process subdirectories recursively
    for subdir in "$dir"/*/; do
        [ -d "$subdir" ] || continue
        local dirname=$(basename "$subdir")
        local sub_relative_path="$relative_path$dirname/"
        collect_all_textures "$subdir" "$sub_relative_path"
    done
}

# Generate the C# file
cat > "$destinationPath" << 'EOF'
using System.Collections.Generic;

namespace AstroMiner.Definitions
{
    public static class Tx
    {
EOF

# Generate class content
generate_class_content "$sourceDirectory" "        " "" >> "$destinationPath"

# Generate AllTextures list
echo "" >> "$destinationPath"
echo "        public static readonly List<string> AllTextures = new List<string>" >> "$destinationPath"
echo "        {" >> "$destinationPath"
collect_all_textures "$sourceDirectory" "" >> "$destinationPath"
echo "        };" >> "$destinationPath"

# Close the class and namespace
cat >> "$destinationPath" << 'EOF'
    }
}
EOF

echo "Generated $destinationPath"