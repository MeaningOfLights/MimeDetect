# MimeDetect
Detects real file formats based on binary analysis. Usage:

C#
```
private static MimeType GetMimeType(string filePath)
{
   MimeTypes mimeTypes As New MimeTypes();
   return mimeTypes.GetMimeTypeFromFile(filePath);
}

VB.Net
Private Shared Function GetMimeType(ByVal filePath As String) As MimeType
   Dim _MimeTypes As New MimeTypes()
   Return _MimeTypes.GetMimeTypeFromFile(filePath)
End Function
```
REF: https://stackoverflow.com/questions/15300567/alternative-to-findmimefromdata-method-in-urlmon-dll-one-which-has-more-mime-typ
