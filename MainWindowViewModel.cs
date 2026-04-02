using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NetWatcher.App;

public sealed class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly List<MarriageReasonItem> _allReasons;
    private readonly BirthdayEasterEgg? _birthdayEasterEgg;
    private readonly IReadOnlyList<ReasonCategoryOption> _categories;
    private MarriageReasonItem _highlightedReason;
    private ReasonCategoryOption _selectedCategory;
    private string _copyStatus = "挑一段順眼的，再按複製。";

    public MainWindowViewModel()
    {
        _birthdayEasterEgg = BirthdayEasterEgg.CreateFor(DateTime.Today);

        _categories =
        [
            new("全部故事", "一次看完整個最瞎結婚理由宇宙"),
            new("鋒兄篇", "鋒兄、思敏與頭獎號碼牽出的婚事"),
            new("塗哥篇", "塗哥、蕙瑄與今彩五三九的同款玄學"),
            new("收尾篇", "把整件事收成最荒謬也最甜的結論")
        ];

        _allReasons =
        [
            new(
                "鋒兄篇",
                "鋒兄啊你說真的還假的",
                "鋒兄啊你說真的還假的，塗哥聽了都快笑翻了。鋒兄說要結婚，理由只有一個，今彩五三九開獎那天，頭獎號碼是思敏給的。看著獎金直直落，心也跟著被收編，他說這是命中注定，不娶怎麼對得起這一連串的玄。",
                "頭獎牽線"
            ),
            new(
                "鋒兄篇",
                "史上最瞎結婚理由",
                "史上最瞎結婚理由，今彩五三九牽紅線牽這麼兇。一個思敏一個蕙瑄，號碼一簽兩人都中頭獎圈。你說愛情是運氣還是數學題，笑到流淚也只能說一句，最瞎最瞎卻又有點甜蜜。",
                "甜到離譜"
            ),
            new(
                "塗哥篇",
                "換到塗哥這邊",
                "換到塗哥這邊，故事居然同一套。今彩五三九播報畫面一出來，他整個人直接跳。蕙瑄隨手寫的牌，竟然全中好幾排。他說財神爺都點名了，不跟她走進禮堂實在太不應該。",
                "同款玄學"
            ),
            new(
                "收尾篇",
                "喝喜酒的人一桌一桌",
                "鋒兄牽著思敏，塗哥牽著蕙瑄。喝喜酒的人一桌一桌，還在笑這兩段緣。最瞎結婚理由，結果都開成頭獎。如果幸福也能這樣瞎忙，那我明天也去買一張。",
                "笑著收尾"
            )
        ];

        VisibleReasons = new ObservableCollection<MarriageReasonItem>();
        _selectedCategory = _categories[0];
        _highlightedReason = _allReasons[0];

        RandomizeReasonCommand = new RelayCommand(SelectRandomReason);
        NextReasonCommand = new RelayCommand(SelectNextReason);

        RefreshVisibleReasons();
    }

    public ObservableCollection<MarriageReasonItem> VisibleReasons { get; }

    public IReadOnlyList<ReasonCategoryOption> Categories => _categories;

    public ICommand RandomizeReasonCommand { get; }

    public ICommand NextReasonCommand { get; }

    public bool IsBirthdayEasterEggVisible => _birthdayEasterEgg is not null;

    public string BirthdayBadge => _birthdayEasterEgg?.Badge ?? string.Empty;

    public string BirthdayHeadline => _birthdayEasterEgg?.Headline ?? string.Empty;

    public string BirthdaySubheadline => _birthdayEasterEgg?.Subheadline ?? string.Empty;

    public string BirthdayHighlight => _birthdayEasterEgg?.Highlight ?? string.Empty;

    public string BirthdaySupportLine => _birthdayEasterEgg?.SupportLine ?? string.Empty;

    public ReasonCategoryOption SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                RefreshVisibleReasons();
            }
        }
    }

    public MarriageReasonItem HighlightedReason
    {
        get => _highlightedReason;
        set
        {
            if (SetProperty(ref _highlightedReason, value))
            {
                RaisePropertyChanged(nameof(ShareText));
                RaisePropertyChanged(nameof(SelectionSummary));
            }
        }
    }

    public string HeroDescription
    {
        get
        {
            var baseText =
                "這個版本直接以你提供的文案為主軸，保留鋒兄、塗哥、思敏、蕙瑄與今彩五三九這條最荒謬也最完整的敘事線。"
                + " 不是單純一句笑話，而是一整套聽起來很扯、偏偏又甜得很順的結婚理由。";

            if (_birthdayEasterEgg is null)
            {
                return baseText;
            }

            return baseText + " 今天也會依日期自動加開生日彩蛋特效。";
        }
    }

    public string CategoryDescription => SelectedCategory.Description;

    public string SelectionSummary =>
        $"目前篇章：{SelectedCategory.Name}。這一區共有 {VisibleReasons.Count} 段故事，現在看到的是「{HighlightedReason.Title}」。";

    public string ShareText =>
        $"{HighlightedReason.Title}\n\n{HighlightedReason.Reason}\n\n標籤：{HighlightedReason.Vibe}";

    public string CopyStatus
    {
        get => _copyStatus;
        private set => SetProperty(ref _copyStatus, value);
    }

    public void SetCopyStatus(string message)
    {
        CopyStatus = message;
    }

    public void Dispose()
    {
    }

    private void RefreshVisibleReasons()
    {
        VisibleReasons.Clear();

        foreach (var item in FilteredReasons())
        {
            VisibleReasons.Add(item);
        }

        HighlightedReason = VisibleReasons.FirstOrDefault() ?? _allReasons[0];
        CopyStatus = "挑一段順眼的，再按複製。";
        RaisePropertyChanged(nameof(CategoryDescription));
        RaisePropertyChanged(nameof(SelectionSummary));
    }

    private IEnumerable<MarriageReasonItem> FilteredReasons()
    {
        if (SelectedCategory.Name == "全部故事")
        {
            return _allReasons;
        }

        return _allReasons.Where(item => item.Category == SelectedCategory.Name);
    }

    private void SelectRandomReason()
    {
        if (VisibleReasons.Count == 0)
        {
            return;
        }

        HighlightedReason = VisibleReasons[Random.Shared.Next(VisibleReasons.Count)];
        CopyStatus = "已切到另一段更瞎的版本。";
    }

    private void SelectNextReason()
    {
        if (VisibleReasons.Count == 0)
        {
            return;
        }

        var currentIndex = VisibleReasons.IndexOf(HighlightedReason);
        var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % VisibleReasons.Count;
        HighlightedReason = VisibleReasons[nextIndex];
        CopyStatus = "已切到下一段故事。";
    }
}

public sealed record ReasonCategoryOption(string Name, string Description);

public sealed record MarriageReasonItem(string Category, string Title, string Reason, string Vibe);

public sealed record BirthdayEasterEgg(
    string Badge,
    string Headline,
    string Subheadline,
    string Highlight,
    string SupportLine)
{
    public static BirthdayEasterEgg? CreateFor(DateTime today)
    {
        return (today.Month, today.Day) switch
        {
            (4, 3) => new BirthdayEasterEgg(
                "4 月 3 日彩蛋",
                "塗哥生日快樂特效已啟動",
                "今天一打開頁面就送上生日彩蛋，主角是塗哥，旁邊還要把今彩539頭獎得主鋒兄一起請上場。",
                "今彩539頭獎得主鋒兄",
                "塗哥生日快樂"),
            (11, 27) => new BirthdayEasterEgg(
                "11 月 27 日彩蛋",
                "鋒兄生日快樂特效已啟動",
                "每年 11 月 27 日自動切到鋒兄主場模式，讓頁面直接高調祝壽，並補上他的榜首稱號。",
                "高考三級資訊處理榜首鋒兄",
                "鋒兄生日快樂"),
            _ => null
        };
    }
}

public sealed class RelayCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute();
}
