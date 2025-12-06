"""
Convert and resize logo to 256x256 PNG for NuGet package
"""
from PIL import Image
import sys
import os

def convert_logo(input_path, output_path, size=(256, 256)):
    """
    Convert image to PNG with proper size for NuGet

    Args:
        input_path: Path to source image
        output_path: Path where to save the icon.png
        size: Tuple of (width, height), default 256x256
    """
    try:
        # Open the image
        img = Image.open(input_path)

        # Convert to RGBA if not already (for transparency support)
        if img.mode != 'RGBA':
            img = img.convert('RGBA')

        # Resize to exact dimensions
        img_resized = img.resize(size, Image.Resampling.LANCZOS)

        # Save as PNG
        img_resized.save(output_path, 'PNG', optimize=True)

        print(f"✅ Logo converted successfully!")
        print(f"   Input:  {input_path}")
        print(f"   Output: {output_path}")
        print(f"   Size:   {size[0]}x{size[1]} pixels")

        return True

    except Exception as e:
        print(f"❌ Error converting logo: {e}")
        return False

if __name__ == "__main__":
    # Default paths
    if len(sys.argv) > 1:
        input_image = sys.argv[1]
    else:
        print("Usage: python convert_logo.py <input_image_path>")
        print("\nOr provide the path when prompted:")
        input_image = input("Enter path to logo image: ").strip('"')

    output_path = r"F:\Dynamic\IndFusion\IndFusion.Ember\IndFusion.Ember\code\src\IndFusion.Ember\icon.png"

    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_path), exist_ok=True)

    # Convert
    if convert_logo(input_image, output_path):
        print(f"\n🔥 Logo is ready for NuGet package!")
    else:
        print(f"\n❌ Failed to convert logo")
        sys.exit(1)
