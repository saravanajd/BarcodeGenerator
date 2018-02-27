# Barcode Generator

### Overview ###
This library was designed to generate barcode images from a string of data.

|   Supported   | 
| :------------- |
| EAN-8       | 
| EAN-13        |
| UPC-A         |
| UPC-E           | 
| Codabar          |

### Example ###
```
BarcodeGenerator.Barcode barcode = new BarcodeGenerator.Barcode("978020137962",
                                                BarcodeGenerator.SymbologyType.EAN13);
Image imgBarcode = barcode.DrawBarcode();
```

## Screenshot

![barcodegenerator](https://user-images.githubusercontent.com/19513970/36725561-acf9fc84-1bdc-11e8-9d52-8e1984af0207.png)

