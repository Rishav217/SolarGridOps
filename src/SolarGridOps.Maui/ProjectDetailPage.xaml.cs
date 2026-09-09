using Microsoft.Extensions.DependencyInjection;
using SolarGridOps.Maui.Services;

namespace SolarGridOps.Maui;

public partial class ProjectDetailPage : ContentPage
{
    private readonly ProjectsApiClient _projectsApiClient;
    private readonly CustomersApiClient _customersApiClient;
    private readonly InventoryApiClient _inventoryApiClient;
    private readonly InstallationApiClient _installationApiClient;
    private readonly Guid _projectId;

    public ProjectDetailPage(Guid projectId)
    {
        InitializeComponent();
        _projectId = projectId;
        _projectsApiClient = App.Services.GetRequiredService<ProjectsApiClient>();
        _customersApiClient = App.Services.GetRequiredService<CustomersApiClient>();
        _inventoryApiClient = App.Services.GetRequiredService<InventoryApiClient>();
        _installationApiClient = App.Services.GetRequiredService<InstallationApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            StatusLabel.Text = "Loading project details...";

            var project = await _projectsApiClient.GetByIdAsync(_projectId);
            if (project is null)
            {
                StatusLabel.Text = "Project not found.";
                return;
            }

            ProjectCodeLabel.Text = project.ProjectCode;
            ProjectMetaLabel.Text = $"{project.CurrentPhase} | {project.CapacityKW} kW";

            var customerTask = _customersApiClient.GetByIdAsync(project.CustomerId);
            var panelsTask = _inventoryApiClient.GetPanelsAsync(_projectId);
            var sessionsTask = _installationApiClient.GetSessionsAsync(_projectId);

            await Task.WhenAll(customerTask, panelsTask, sessionsTask);

            var customer = customerTask.Result;
            if (customer is not null)
            {
                CustomerNameLabel.Text = customer.FullName;
                CustomerPhoneLabel.Text = $"Phone: {customer.PhoneNumber}";
                CustomerAddressLabel.Text = $"{customer.Address}, {customer.City}, {customer.State}";
            }
            else
            {
                CustomerNameLabel.Text = "Customer record not found.";
                CustomerPhoneLabel.Text = string.Empty;
                CustomerAddressLabel.Text = string.Empty;
            }

            var panels = panelsTask.Result;
            PanelCountLabel.Text = $"{panels.Count} panel(s)";
            PanelsCollectionView.ItemsSource = panels
                .Select(p => new PanelRow
                {
                    SerialNumber = p.SerialNumber,
                    SummaryLine = $"{p.Wattage}W | {p.Brand ?? "Brand not recorded"}"
                })
                .ToList();

            var sessions = sessionsTask.Result;
            SessionsCollectionView.ItemsSource = sessions
                .Select(s => new SessionRow
                {
                    DateLine = $"{s.SessionDateUtc:yyyy-MM-dd} | {(s.IsCompletedForDay ? "Day completed" : "In progress")}",
                    TechnicianLine = $"Technician: {(string.IsNullOrWhiteSpace(s.TechnicianName) ? "Not yet assigned" : s.TechnicianName)}",
                    SummaryLine = string.IsNullOrWhiteSpace(s.WorkSummary) ? "No work summary recorded." : s.WorkSummary,
                    StatusLine = string.IsNullOrWhiteSpace(s.ClosureStatus) ? string.Empty : $"Closure status: {s.ClosureStatus}"
                })
                .ToList();

            StatusLabel.Text = "Project details loaded.";
        }
        catch
        {
            StatusLabel.Text = "Unable to load full project details. Check server connection.";
        }
    }

    private async void OnOpenStockClicked(object? sender, EventArgs e)
    {
        var encodedProjectId = Uri.EscapeDataString(_projectId.ToString());
        await Shell.Current.GoToAsync($"//app/inventory?projectId={encodedProjectId}");
    }

    private sealed class PanelRow
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string SummaryLine { get; set; } = string.Empty;
    }

    private sealed class SessionRow
    {
        public string DateLine { get; set; } = string.Empty;
        public string TechnicianLine { get; set; } = string.Empty;
        public string SummaryLine { get; set; } = string.Empty;
        public string StatusLine { get; set; } = string.Empty;
    }
}
