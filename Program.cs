/*
    ttfs2mix TTFS to MIX utility
    Copyright (C) 2021 Unstoppable

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

namespace Ttfs2Mix;

[Description("Converts first occurence of TTFS package to MIX file and saves into data folder.")]
public class ConvertCommand : Command<ConvertCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<package id/name>")]
        [Description("ID or name of the package.")]
        public string Package { get; set; }
    }
    
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Ttfs2Mix.Convert(settings.Package)
            .GetAwaiter()
            .GetResult();

        return 0;
    }
}

[Description("Converts first occurence of TTFS package to MIX file and saves into data folder.")]
public class ConvertAllCommand : Command<ConvertAllCommand.Settings>
{
    public class Settings : CommandSettings
    {
        
    }
    
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Ttfs2Mix.ConvertAll()
            .GetAwaiter()
            .GetResult();

        return 0;
    }
}

[Description("Converts all matching TTFS packages to MIX files and saves into data folder.")]
public class MultiConvertCommand : Command<MultiConvertCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<package id/name>")]
        [Description("Partial or full ID or name of the package.")]
        public string PackageMatch { get; set; }
    }
    
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Ttfs2Mix.MultiConvert(settings.PackageMatch)
            .GetAwaiter()
            .GetResult();

        return 0;
    }
}

[Description("Finds and downloads first occurence of TTFS package from a remote repository to MIX file and saves into data folder.")]
public class DownloadCommand : Command<DownloadCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<package id/name>")]
        [Description("ID or name of the package.")]
        public string Package { get; set; }
        
        [CommandArgument(1, "<url>")]
        [Description("Location of the TTFS repository.")]
        public string Location { get; set; }
        
        [CommandOption("-n|--count")]
        [Description("Number of maximum allowed concurrent file downloads for a package.")]
        [DefaultValue(1)]
        public int Count { get; set; }
    }
    
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Ttfs2Mix.Download(settings.Package, settings.Location, settings.Count)
            .GetAwaiter()
            .GetResult();

        return 0;
    }
}

[Description("Finds and downloads all TTFS packages from a remote repository to MIX files and saves all into data folder.")]
public class DownloadAllCommand : Command<DownloadAllCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<url>")]
        [Description("Location of the TTFS repository.")]
        public string Location { get; set; }
        
        [CommandOption("-n|--count")]
        [Description("Number of maximum allowed concurrent file downloads for a package.")]
        [DefaultValue(1)]
        public int Count { get; set; }
    }
    
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine("Count is:  " + settings.Count);
        Ttfs2Mix.DownloadAll(settings.Location, settings.Count)
            .GetAwaiter()
            .GetResult();

        return 0;
    }
}

[Description("Finds and downloads all matching TTFS packages from a remote repository to MIX files and saves into data folder.")]
public class MultiDownloadCommand : Command<MultiDownloadCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<package id/name>")]
        [Description("ID or name of the package.")]
        public string Package { get; set; }
        
        [CommandArgument(1, "<url>")]
        [Description("Location of the TTFS repository.")]
        public string Location { get; set; }
        
        [CommandOption("-n|--count")]
        [Description("Number of maximum allowed concurrent file downloads for a package.")]
        [DefaultValue(1)]
        public int Count { get; set; }
    }
    
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Ttfs2Mix.MultiDownload(settings.Package, settings.Location, settings.Count)
            .GetAwaiter()
            .GetResult();

        return 0;
    }
}

public class Ttfs2MixHelpProvider : HelpProvider
{
    public Ttfs2MixHelpProvider(ICommandAppSettings settings) : base(settings)
    {
        
    }

    public override IEnumerable<IRenderable> GetHeader(ICommandModel model, ICommandInfo command)
    {
        var list = base.GetHeader(model, command)
            .ToList();
        
        list.AddRange(new Rule(),
            new FigletText(Ttfs2Mix.Name),
            new Markup($"{Ttfs2Mix.Name} is an application to convert TTFS packages back to MIX/PKG files. Supports conversions on both client and FDS." + Environment.NewLine),
            new Markup($"{Ttfs2Mix.Name} has to be placed inside the client or FDS folder, alongside 'game.exe' or 'server.exe'. Converted maps will be saved to the Data folder." + Environment.NewLine),
            new Markup(Environment.NewLine),
            new Markup($"[blue]{Ttfs2Mix.Name}[/] is licensed under [bold]GNU General Public License v3.0[/]. Please view [b]LICENSE[/] file for details." + Environment.NewLine),
            new Markup($"[blue]{Ttfs2Mix.Name}[/] uses the following open-source libraries and code snippets:" + Environment.NewLine),
            new Rows(
                new Markup("[bold]MixLibrary[/] [dim]by[/] The Unstoppable"),
                new Markup("[bold]Spectre.Console.Cli[/] [dim]by[/] Patrik Svensson, Phil Scott, Nils Andresen, Cédric Luthi"),
                new Markup("[bold]TTPackageClass[/] [dim]by[/] The Unstoppable"),
                new Markup("https://stackoverflow.com/a/14488941/5791443")
            ),
            new Rule()
        );

        return list;
    }

    public override IEnumerable<IRenderable> GetFooter(ICommandModel model, ICommandInfo command)
    {
        return base.GetFooter(model, command).Append(new Rule());
    }
}

public class Ttfs2Mix
{
    public const string Name = nameof(Ttfs2Mix);
    public const string Version = "1.3";
    public const string Authors = "Unstoppable";

    public static void PrintSplash()
    {
        Console.WriteLine($"{Name} utility {Version} - by {Authors}");
    }
    
    public static int Main(string[] args)
    {
        PrintSplash();

        var app = new CommandApp();
        
        app.Configure(x =>
        {
            x.SetApplicationName(Name);
            x.SetApplicationVersion(Version);
            x.SetHelpProvider(new Ttfs2MixHelpProvider(x.Settings));
            
            x.AddCommand<ConvertCommand>("convert");
            x.AddCommand<ConvertAllCommand>("convertall");
            x.AddCommand<MultiConvertCommand>("multiconvert");
            x.AddCommand<DownloadCommand>("download");
            x.AddCommand<DownloadAllCommand>("downloadall");
            x.AddCommand<MultiDownloadCommand>("multidownload");
        });
        
        return app.Run(args);
    }
    
    #region Helpers
    static PathsStruct? Paths;
    static string TTFSFolder;

    private static bool PackageIDCheck(TPIPackageClass Package, string ID)
    {
        return Package.PackageID
            .ToLower(Data.DefaultCulture)
            .Equals(ID.ToLower(Data.DefaultCulture));
    }

    private static bool PackageNameCheck(TPIPackageClass Package, string Name)
    {
        return Package.PackageName
            .Equals(Name);
    }

    private static bool CheckFields(out TTFSDataClass? TTFSData)
    {
        TTFSData = null;

        Paths ??= Data.ReadPaths();
        TTFSFolder ??= Data.GetTTFSDirectory(Paths.Value);

        if (!TTFSData.HasValue)
        {
            if (TTFSFolder != null)
            {
                try
                {
                    TTFSData = TTFSClass.FromFile(Path.Combine(TTFSFolder, "packages.dat"));
                }
                catch (Exception ex)
                {
                    AnsiConsole.Exception(ex);
                    return false;
                }
            }
            else
            {
                AnsiConsole.ErrorLine($"Could not auto-detect TTFS folder. Please make sure you are running this utility from root directory game/server folder.");
                return false;
            }
        }
        else
        {
            TTFSData = TTFSData.Value;
        }

        return true;
    }
    
    private static void FindPackageFunc(string Package, TTFSDataClass TTFS, out TPIPackageClass? TPI)
    {
        TPI = null;
        var IDMatch = TTFS.Packages.FindAll(x => PackageIDCheck(x, Package));
        var NameMatch = TTFS.Packages.FindAll(x => PackageNameCheck(x, Package));

        if (IDMatch.Count == 1) //ID Match
        {
            TPI = IDMatch.First();
        }
        else if (NameMatch.Count == 1) //Name Match
        {
            TPI = NameMatch.First();
        }
        else if (IDMatch.Count > 1) //Too many ID match
        {
            AnsiConsole.ErrorLine($"Too many matches found with specified identifier \"{Package}\".");
        }
        else if (NameMatch.Count > 1) //Too many Name match
        {
            AnsiConsole.ErrorLine($"Too many matches found with specified identifier \"{Package}\".");
        }
        else
        {
            AnsiConsole.ErrorLine($"Couldn't find any package with specified identifier \"{Package}\".");
        }
    }
    
    private static void SaveFunc(MixPackageClass MIXPackage, TPIPackageClass TPI, ProgressContext ctx)
    {
        var task = ctx.AddTask($"Saving {TPI.PackageName}...");
        task.IsIndeterminate = true;

        MIXPackage.Save(out var ms);

        ms.Position = 0;
        task.MaxValue = ms.Length;
        task.IsIndeterminate = false;

        var isMod = MIXPackage.Files.Count(x => x.FileName.EndsWith(".lsd", StringComparison.OrdinalIgnoreCase)) > 1;
        var loc = Path.Combine(Data.WorkingDirectory, "data", $"{TPI.PackageName}.{(isMod ? "pkg" : "mix")}");
        using var fs = new FileStream(loc, FileMode.Create, FileAccess.Write);
                
        fs.SetLength(ms.Length);
                
        int bytesRead;
        byte[] buffer = new byte[81920];
        while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
        {
            fs.Write(buffer, 0, bytesRead);
            task.Increment(bytesRead);
        }
                
        ms.Dispose();
        ctx.RemoveTask(task);
    }

    private static int ExceptionToHTTPCode(HttpRequestException ex)
    {
        return ex.StatusCode.HasValue ? (int)ex.StatusCode.Value : -1;
    }

    private static void PrintHTTPErrorInfo(int Status)
    {
        switch (Status)
        {
            case 401: //Forbidden
                AnsiConsole.ErrorLine($"Authorization required to view this location. Please make sure server allows ttfs2mix or you have permission to access.");
                break;
            case 404: //Not found
                AnsiConsole.ErrorLine($"Unable to find a TTFS repository at the specified location. Please make sure entered URL is correct.");
                break;
            case 403: //Forbidden
                AnsiConsole.ErrorLine($"Server forbid access to this location. Please make sure server allows ttfs2mix or you have permission to access.");
                break;
            case 500: //Server error
                AnsiConsole.ErrorLine($"An internal server error occured while requesting TTFS packages. Please try again later.");
                break;
            default:
                AnsiConsole.ErrorLine($"Server sent an unrecognized status code of {Status}. Please make sure server allows ttfs2mix, or try again later.");
                break;
        }
    }
    #endregion

    #region Commands

    internal static async Task InternalConvertPackageAsync(TPIPackageClass TPI, ProgressContext? Context = null)
    {
        MixPackageClass MIXPackage = MixPackageClass.CreateMIX();
        
        void ConversionFunc(ProgressContext ctx)
        {
            var task = ctx.AddTask(string.IsNullOrWhiteSpace(TPI.PackageName) ? TPI.PackageID : TPI.PackageName, maxValue: TPI.FileCount);
                
            foreach(TTFileClass TTFile in TPI.Files)
            {
                var fileTask = ctx.AddTask(TTFile.FileName);
                fileTask.IsIndeterminate = true;
                    
                try
                {
                    using var ms = new MemoryStream();
                    using var fs = File.OpenRead(Path.Combine(TTFSFolder, "files", TTFile.FullName.Replace("\\", "_")));

                    fileTask.MaxValue = fs.Length;
                    fileTask.IsIndeterminate = false;
                        
                    int bytesRead;
                    byte[] buffer = new byte[81920];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                        fileTask.Increment(bytesRead);
                    }
                        
                    MIXPackage.Files.Add(new MixFileClass
                    {
                        FileName = TTFile.FileName,
                        Data = ms.ToArray()
                    });
                }
                catch(Exception ex)
                {
                    AnsiConsole.ErrorLine($"Failed to process file '{TTFile.FileName}' in '{TPI.PackageName}' ({TPI.PackageID}).");
                    AnsiConsole.Exception(ex);
                }

                ctx.RemoveTask(fileTask);
                task.Increment(1);
            }

            ctx.RemoveTask(task);
        }
        
        if (Context != null)
        {
            ConversionFunc(Context);
            SaveFunc(MIXPackage, TPI, Context);
        }
        else
        {
            AnsiConsole.Progress()
                .AutoClear(true)
                .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
                .Start(ConversionFunc);
            
            AnsiConsole.Progress()
                .AutoClear(true)
                .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
                .Start(ctx => SaveFunc(MIXPackage, TPI, ctx));
        }

        AnsiConsole.MarkupLine($"[green][bold]+[/][/] {TPI.PackageName} [dim]({TPI.PackageID})[/]");
    }
    
    internal static async Task InternalDownloadPackageAsync(string Location, TPIPackageClass TPI, int Count, ProgressContext? Context = null)
    {
        MixPackageClass MIXPackage = MixPackageClass.CreateMIX();
        
        async Task DownloadFilesFunc(ProgressContext ctx)
        {
            Dictionary<Task, ProgressTask> DownloadTasks = new();
            var task = ctx.AddTask(string.IsNullOrWhiteSpace(TPI.PackageName) ? TPI.PackageID : TPI.PackageName, maxValue: TPI.FileCount);
            
            async Task WaitDownload()
            {
                var finishedTask = await Task.WhenAny(DownloadTasks.Keys);
                await finishedTask;
                    
                ctx.RemoveTask(DownloadTasks[finishedTask]);
                DownloadTasks.Remove(finishedTask);
                task.Increment(1);
            }
            
            foreach(TTFileClass File in TPI.Files)
            {
                if (DownloadTasks.Count >= Count)
                {
                    await WaitDownload();
                }
                
                var fileTask = ctx.AddTask($"{File.FileName} ({WebDownloader.ParseSize(File.FileSize)} - 0.00 bytes/s)", maxValue: File.FileSize);
                DownloadTasks.Add(DownloadFile(File, fileTask), fileTask);
            }

            while (DownloadTasks.Count > 0)
            {
                await WaitDownload();
            }
            
            ctx.RemoveTask(task);
        }

        async Task DownloadFile(TTFileClass File, ProgressTask Task)
        {
            byte[] Data;
            try
            {
                Data = await WebDownloader.GetBytesAsync($"{Location}/files/{Uri.EscapeDataString($"{File.CRC}.{File.FileName.Replace('\\', '_')}")}", TPI.PackageName, new Progress<WebDownloaderProgress>(x =>
                {
                    Task.Description = $"{File.FileName} ({WebDownloader.ParseSize(File.FileSize)} - {WebDownloader.ParseSize(x.Speed)}/s)";
                    Task.Value = x.DownloadedBytes;
                    Task.MaxValue = x.TotalSize;
                }));

                lock (MIXPackage.Files)
                {
                    MIXPackage.Files.Add(new MixFileClass
                    {
                        FileName = File.FileName,
                        Data = Data
                    });
                }
            }
            catch(Exception ex)
            {
                AnsiConsole.ErrorLine($"Skipping file '{File.FileName}' in package '{TPI.PackageName}'.");
                AnsiConsole.Exception(ex);
            }
        }

        if (Context != null)
        {
            await DownloadFilesFunc(Context);
            SaveFunc(MIXPackage, TPI, Context);
        }
        else
        {
            await AnsiConsole.Progress()
                .AutoClear(true)
                .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
                .StartAsync(DownloadFilesFunc);
            
            AnsiConsole.Progress()
                .AutoClear(true)
                .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
                .Start(ctx => SaveFunc(MIXPackage, TPI, ctx));
        }

        GC.Collect();

        AnsiConsole.MarkupLine($"[green][bold]+[/][/] {TPI.PackageName} [dim]({TPI.PackageID})[/]");
    }
    
    internal static async Task<TTFSDataClass?> InternalFetchPackagesAsync(string Location)
    {
        try
        {
            byte[] Data = await WebDownloader.GetBytesAsync($"{Location}/packages.dat", string.Empty);
            return TTFSClass.FromBytes(Data);
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException httpex)
            {
                var Status = ExceptionToHTTPCode(httpex);

                if (Status > 0)
                {
                    PrintHTTPErrorInfo(Status);
                    return null;
                }
            }
            
            AnsiConsole.Exception(ex);
            return null;
        }
    }
    
    internal static async Task Convert(string Package)
    {
        if (!CheckFields(out var TTFSData) || TTFSData is not { } TTFS)
        {
            return;
        }

        TPIPackageClass? TPI = null;
        
        AnsiConsole.Status()
            .Start("Locating...", ctx =>
            {
                FindPackageFunc(Package, TTFS, out TPI);
            });

        if (!TPI.HasValue)
        {
            return;
        }

        await InternalConvertPackageAsync(TPI.Value);
    }

    internal static async Task ConvertAll()
    {
        if (!CheckFields(out var TTFSData) || TTFSData is not { } TTFS)
        {
            return;
        }

        await AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Converting all packages...", maxValue: TTFS.PackageCount);

                foreach (TPIPackageClass Package in TTFS.Packages)
                {
                    await InternalConvertPackageAsync(Package, ctx);
                    task.Increment(1);
                }
            });
    }
    
    internal static async Task MultiConvert(string Package)
    {
        if (!CheckFields(out var TTFSData) || TTFSData is not { } TTFS)
        {
            return;
        }

        TPIPackageClass[] Matches = [];

        AnsiConsole.Status()
            .Start("Locating...", ctx =>
            {
                Matches = TTFS.Packages.Where(x =>
                        x.PackageName.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)) ||
                        x.PackageID.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)))
                    .ToArray();
            });
        
        if(Matches.Length == 0)
        {
            AnsiConsole.ErrorLine($"Couldn't find any package with specified identifier \"{Package}\".");
            return;
        }
        
        await AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Converting packages...", maxValue: Matches.Length);

                foreach (TPIPackageClass Package in Matches)
                {
                    await InternalConvertPackageAsync(Package, ctx);
                    task.Increment(1);
                }
            });
    }
    
    internal static async Task Download(string Package, string Location, int Count)
    {
        if (await InternalFetchPackagesAsync(Location) is not { } TTFS)
        {
            return;
        }

        TPIPackageClass? TPI = null;
        
        AnsiConsole.Status()
            .Start("Locating...", ctx =>
            {
                FindPackageFunc(Package, TTFS, out TPI);
            });

        if (!TPI.HasValue)
        {
            return;
        }

        await InternalDownloadPackageAsync(Location, TPI.Value, Count);
    }

    internal static async Task MultiDownload(string Package, string Location, int Count)
    {
        if (await InternalFetchPackagesAsync(Location) is not { } TTFS)
        {
            return;
        }

        TPIPackageClass[] Matches = [];

        AnsiConsole.Status()
            .Start("Locating...", ctx =>
            {
                Matches = TTFS.Packages.Where(x =>
                        x.PackageName.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)) ||
                        x.PackageID.ToLower(Data.DefaultCulture).Contains(Package.ToLower(Data.DefaultCulture)))
                    .ToArray();
            });
        
        if(Matches.Length == 0)
        {
            AnsiConsole.ErrorLine($"Couldn't find any package with specified identifier \"{Package}\".");
            return;
        }
        
        await AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Downloading packages...", maxValue: Matches.Length);

                foreach (TPIPackageClass Package in Matches)
                {
                    await InternalDownloadPackageAsync(Location, Package, Count, ctx);
                    task.Increment(1);
                }
            });
    }
    
    internal static async Task DownloadAll(string Location, int Count)
    {
        if (await InternalFetchPackagesAsync(Location) is not { } TTFS)
        {
            return;
        }

        await AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(new SpinnerColumn(), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Downloading all packages...", maxValue: TTFS.PackageCount);

                foreach (TPIPackageClass Package in TTFS.Packages)
                {
                    await InternalDownloadPackageAsync(Location, Package, Count, ctx);
                    task.Increment(1);
                }
            });
    }

    #endregion
}