using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class ProjectsListPage : ContentPage
{
    private readonly ProjectsApiClient _projectsApiClient;

    public ProjectsListPage()
    {
        InitializeComponent();
        _projectsApiClient = App.Services.GetRequiredService<ProjectsApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync();
        ProjectsRefreshView.IsRefreshing = false;
    }

    private async Task LoadAsync()
    {
        try
        {
            StatusLabel.Text = "Loading projects...";
            var projects = await _projectsApiClient.ListAsync();
            ProjectsCollectionView.ItemsSource = projects
                .Select(p => new ProjectRow
                {
                    Id = p.Id,
                    ProjectCode = p.ProjectCode,
                    CustomerName = p.CustomerName,
                    SiteAddress = string.IsNullOrWhiteSpace(p.SiteAddress) ? "Site address not recorded" : p.SiteAddress,
                    PhaseDisplay = FormatPhase(p.CurrentPhase),
                    CapacityDisplay = $"{p.CapacityKW} kW"
                })
                .ToList();
            StatusLabel.Text = $"{projects.Count} project(s) found.";
        }
        catch
        {
            StatusLabel.Text = "Unable to load projects. Check server connection.";
        }
    }

    private async void OnProjectSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ProjectRow selected)
        {
            return;
        }

        ProjectsCollectionView.SelectedItem = null;
        await Navigation.PushAsync(new ProjectDetailPage(selected.Id));
    }

    private static string FormatPhase(string phase)
    {
        return string.IsNullOrWhiteSpace(phase) ? "Unknown Phase" : phase;
    }

    private sealed class ProjectRow
    {
        public Guid Id { get; set; }
        public string ProjectCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string SiteAddress { get; set; } = string.Empty;
        public string PhaseDisplay { get; set; } = string.Empty;
        public string CapacityDisplay { get; set; } = string.Empty;
    }
}
