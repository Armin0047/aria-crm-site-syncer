# Aria CRM Site Syncer

سرویس ویندوزی C# برای همگام‌سازی کالاها و فاکتورها بین SQL Server و API سامانه CRM.

## قابلیت‌ها

- خواندن کالای منتظر ارسال از رویه‌ی `dbo.sp_sent_details_to_site` با `@state = 0` و ارسال به مسیر `POST /products`.
- خواندن اقلام فاکتور با `@state = 1` و ارسال به مسیر `POST /invoices`.
- ارسال کلید API در هدر `X-API-KEY`.
- ثبت نتیجه‌ی ارسال در جدول `dbo.LogCrmService` و به‌روزرسانی وضعیت رکوردهای `Kala` و `Sandogh`.

## پیش‌نیازها

- Windows و SQL Server در دسترس سرویس.
- Visual Studio با ابزارهای ساخت پروژه‌های قدیمی .NET Framework، یا MSBuild سازگار.
- پروژه در حال حاضر برای **.NET Framework 4.6** تنظیم شده است. این نسخه به پایان پشتیبانی رسیده؛ برای استقرار جدید، ارتقا و آزمون سازگاری روی نسخه‌ی جدیدتر .NET Framework را در برنامه قرار دهید. پشتیبانی نسخه‌های 4.5.2، 4.6 و 4.6.1 در 26 آوریل 2022 پایان یافته است. [جزئیات چرخه‌ی عمر Microsoft](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework).
- بسته‌ی Newtonsoft.Json نسخه‌ی 13.0.3 (قابل بازیابی با NuGet از `packages.config`).

## تنظیمات

### اتصال SQL Server

کنار فایل اجرایی سرویس، فایلی به نام `Settings.ini` بسازید. سرویس آن را از پوشه‌ی اجرایی می‌خواند:

```ini
[Form]
Server=YOUR_SQL_SERVER
dataBase=YOUR_DATABASE
user=YOUR_SQL_LOGIN
pass=YOUR_SQL_PASSWORD
```

مقادیر نمونه را با اطلاعات واقعی محیط خود جایگزین کنید. فایل واقعی `Settings.ini` را در Git قرار ندهید و دسترسی پوشه‌ی نصب سرویس را محدود کنید.

### تنظیمات API

در جدول `setting` پایگاه‌داده باید این مقادیر موجود باشند:

| `tag` | `key` | کاربرد |
| --- | --- | --- |
| `sitesyncer` | `UrlCRM` | آدرس پایه‌ی API، با یا بدون `/` پایانی |
| `sitesyncer` | `APIKEY` | کلید موردنیاز هدر `X-API-KEY` |

سرویس مسیرهای `products` و `invoices` را به آدرس پایه اضافه می‌کند.

## پیش‌نیازهای پایگاه‌داده

پایگاه‌داده باید رویه‌ی `dbo.sp_sent_details_to_site` را با پارامتر `@state` داشته باشد. خروجی آن باید با ستون‌هایی که سرویس می‌خواند سازگار باشد:

- `@state = 0`: `external_id`, `description`, `rate`, `unit`.
- `@state = 1`: `id`, `external_id`, `phonenumber`, `company`, `date`, `description`, `unit`, `rate`, `qty`.

همچنین سرویس انتظار دارد ستون‌های `Senttosite` و `SenttoSitetime` در جدول‌های `Kala` و `Sandogh` موجود باشند.

جدول لاگ مورد انتظار `dbo.LogCrmService` با ستون‌های `type` (`varchar(10)`)، `success` (`bit`)، `message` (`varchar(250)`) و `result` (`nvarchar(max)`) است. فایل SQL فعلی در پوشه‌ی خروجی Debug قرار دارد؛ پیش از استقرار، اسکریپت رویه و تغییرات پایگاه‌داده را در محل نسخه‌پذیر و مستقل از `bin/Debug` نگه دارید.

## ساخت

در Windows و از Developer PowerShell/Command Prompt سازگار با MSBuild:

```powershell
dotnet msbuild .\SiteSyncerAria_DrDoosti.sln -t:Build `
  -p:Configuration=Release `
  -p:GenerateResourceMSBuildArchitecture=CurrentArchitecture `
  -p:GenerateResourceMSBuildRuntime=CurrentRuntime
```

این دستور در محیط بررسی‌شده با .NET SDK 9 اجرا شد. فایل‌های خروجی در `SiteSyncerAria_DrDoosti\bin\Release` قرار می‌گیرند. هنگام استقرار، فایل اجرایی، فایل‌های وابسته، فایل تنظیمات محلی و پیکربندی اجرایی را کنار هم قرار دهید.

## نصب و راه‌اندازی

پروژه با `ServiceInstaller` پیکربندی شده و نام سرویس آن `SiteSyncerCRM` است. نصب یا حذف سرویس ویندوز نیازمند دسترسی Administrator است. پیش از نصب روی سرور، تنظیمات محلی و دسترسی SQL/API را بررسی کنید؛ برای نام سرویس موجودی که قبلاً با نام دیگری نصب شده، تغییر سورس به‌تنهایی نام ثبت‌شده در Windows را عوض نمی‌کند.

## ساختار Git

فایل‌های `bin/`, `obj/`, `.vs/`, لاگ‌ها و `Settings.ini` محیطی نباید وارد مخزن شوند. `.gitignore` ریشه این خروجی‌ها و اطلاعات اتصال را کنار می‌گذارد.
