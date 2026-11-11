#define DataFolderName "Ek Ders Hakediş Sistemi"
#ifndef AppVersion
  #define AppVersion "1.0.15"
#endif
#ifndef PayloadDir
  #define PayloadDir "..\dist\v" + AppVersion
#endif
#define AppName "Ek Ders Hakediş Sistemi"
#define AppExeName AppName + ".exe"
#if Ver < EncodeVer(6, 6, 0)
  #error Gece Mavisi kurulumu icin Inno Setup 6.6 veya sonrasi gereklidir.
#endif

[Setup]
AppId={{733BEB22-6C47-49EC-9D19-1404F74058F1}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Murat ÇALIMLIER
AppPublisherURL=https://github.com/mcalimlier/Ek-Ders-Hakedis-Sistemi
AppCopyright=Created by Murat ÇALIMLIER
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
DisableWelcomePage=no
UsePreviousAppDir=yes
UsePreviousTasks=no
OutputDir=..\dist\installer
OutputBaseFilename=Ek-Ders-Hakedis-Sistemi-Kurulum-{#AppVersion}
SetupIconFile=..\src\Hakedis.App\Assets\Hakedis.ico
UninstallDisplayIcon={app}\{#AppExeName}
WizardStyle=modern dark polar includetitlebar hidebevels
WizardBackColor=#10263d
WizardSizePercent=115,115
WizardImageFile=Assets\setup-sidebar.png
WizardSmallImageFile=Assets\setup-mark.png
WizardImageBackColor=#10263d
WizardSmallImageBackColor=#10263d
WizardImageStretch=yes
ShowLanguageDialog=no
#ifdef FastBuild
Compression=zip/1
SolidCompression=no
#else
Compression=lzma2
SolidCompression=yes
#endif
CloseApplications=yes
CloseApplicationsFilter=*.exe
RestartApplications=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
VersionInfoVersion={#AppVersion}

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstünde program kısayolu oluştur"; GroupDescription: "Kısayollar:"

[Files]
; Explicit allowlist: never distribute user databases, exports, or backups.
Source: "{#PayloadDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\KULLANIM.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\NPOI-LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#AppExeName}"
Name: "{userdesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{#AppName} programını başlat"; Flags: nowait postinstall skipifsilent

[Messages]
SetupWindowTitle=Ek Ders Hakediş Sistemi · Kurulum
WelcomeLabel1=Ek Ders Hakediş Sistemi'ne hoş geldiniz
WelcomeLabel2=Bu sihirbaz {#AppName} programını bilgisayarınıza kuracaktır.%n%nÖğretmen kayıtları, aylık ders bilgileri ve bordrolarınızı aynı çalışma ortamında yönetin.%n%nVerileriniz, çıktılarınız, yedekleriniz ve şifre kasanız masaüstünüzdeki "Ek Ders Hakediş Sistemi" klasöründe korunur. Güncelleme ve kaldırma işlemleri bu klasörleri silmez.%n%nDevam etmeden önce açık programda çalışmalarınızı kaydedin.
FinishedHeadingLabel=Ek Ders Hakediş Sistemi kullanıma hazır
FinishedLabel=Kurulum tamamlandı.%n%nProgramı Başlat menüsünden veya oluşturduysanız masaüstü kısayolundan açabilirsiniz. Kayıtlarınız ve şifre kasanız mevcut konumunda korunur.%n%nCreated by Murat ÇALIMLIER

#include "CommonWizard.iss"
