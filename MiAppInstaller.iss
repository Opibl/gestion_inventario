[Setup]
AppName=MiApp Inventory System
AppVersion=1.0
AppPublisher=MiApp
DefaultDirName={pf}\MiApp
DefaultGroupName=MiApp
OutputDir=.
OutputBaseFilename=MiAppSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "D:\Escritorio\MiApp\MiApp.UI\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\MiApp"; Filename: "{app}\MiApp.UI.exe"
Name: "{commondesktop}\MiApp"; Filename: "{app}\MiApp.UI.exe"

[Run]
Filename: "{app}\MiApp.UI.exe"; Description: "Abrir MiApp"; Flags: nowait postinstall skipifsilent

[Code]

var
  DBPage: TInputQueryWizardPage;

procedure InitializeWizard();
begin
  DBPage :=
    CreateInputQueryPage(
      wpSelectDir,
      'Configuración de Base de Datos',
      'Configure la conexión a la base de datos',
      'Ingrese los datos de conexión.');

  DBPage.Add('DB_HOST', False);
  DBPage.Add('DB_PORT', False);
  DBPage.Add('DB_NAME', False);
  DBPage.Add('DB_USER', False);
  DBPage.Add('DB_PASSWORD', True);

  DBPage.Values[0] := 'aws-0-us-west-2.pooler.supabase.com';
  DBPage.Values[1] := '5432';
  DBPage.Values[2] := 'postgres';
  DBPage.Values[3] := '';
  DBPage.Values[4] := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  EnvFile: string;
begin
  if CurStep = ssPostInstall then
  begin
    EnvFile := ExpandConstant('{app}\.env');

    SaveStringToFile(
      EnvFile,
      'DB_HOST=' + DBPage.Values[0] + #13#10 +
      'DB_PORT=' + DBPage.Values[1] + #13#10 +
      'DB_NAME=' + DBPage.Values[2] + #13#10 +
      'DB_USER=' + DBPage.Values[3] + #13#10 +
      'DB_PASSWORD=' + DBPage.Values[4] + #13#10 +
      'POOL_MODE=Session' + #13#10,
      False
    );
  end;
end;