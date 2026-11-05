param([Parameter(Mandatory=$true)][string]$DocumentPath,[Parameter(Mandatory=$true)][string]$PdfPath)
$ErrorActionPreference='Stop'
# Document conversion through installed Word's object model; no SendInput/UI Automation.
$reviewWord=$null
$reviewDocument=$null
$reviewNoSave=0
$reviewConfirm=$false
$reviewReadOnly=$true
$reviewAddToRecent=$false
try {
    $reviewWord=New-Object -ComObject Word.Application
    $reviewWord.Visible=$false
    $reviewWord.DisplayAlerts=0
    $reviewDocument=$reviewWord.Documents.Open([ref]$DocumentPath,[ref]$reviewConfirm,[ref]$reviewReadOnly,[ref]$reviewAddToRecent)
    $reviewDocument.Repaginate()
    $reviewDocument.ExportAsFixedFormat($PdfPath,17)
    Write-Output "Word PDF renderer completed: $PdfPath"
} finally {
    if($null -ne $reviewDocument){$reviewDocument.Close([ref]$reviewNoSave);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($reviewDocument)}
    if($null -ne $reviewWord){$reviewWord.Quit([ref]$reviewNoSave);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($reviewWord)}
}
