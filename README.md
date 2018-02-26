# MimeDetect
Detects real file formats based on binary analysis. Usage:

C#
```Private static MimeType GetMimeType(string filePath)
{
   MimeTypes g_MimeTypes As New MimeTypes();
   return g_MimeTypes.GetMimeTypeFromFile(filePath);
}

VB.Net
Private Shared Function GetMimeType(ByVal filePath As String) As MimeType
   Dim g_MimeTypes As New MimeTypes()
   Return g_MimeTypes.GetMimeTypeFromFile(filePath)
End Function```
