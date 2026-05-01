using Google.Cloud.Firestore;
using ReqNot.WebApi.Models;

namespace ReqNot.WebApi.Services;

public class FirestoreService
{
    private readonly DocumentReference _docRef;

    public FirestoreService(IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"]
            ?? throw new InvalidOperationException("Firebase:ProjectId is not configured.");
        var credentialPath = configuration["Firebase:CredentialPath"]
            ?? throw new InvalidOperationException("Firebase:CredentialPath is not configured.");

        var db = new FirestoreDbBuilder
        {
            ProjectId = projectId,
            CredentialsPath = credentialPath
        }.Build();

        _docRef = db.Collection("reqNot").Document("actual");
    }

    public async Task UpdateWorksAsync(DeviceStatus status)
    {
        await _docRef.SetAsync(
            new Dictionary<string, object>
            {
                ["works"] = (int)status,
                ["lastChecked"] = Timestamp.GetCurrentTimestamp()
            },
            SetOptions.MergeAll);
    }

    public async Task ResetCheckAsync()
    {
        await _docRef.SetAsync(
            new Dictionary<string, object> { ["check"] = false },
            SetOptions.MergeAll);
    }

    public FirestoreChangeListener ListenForCheck(Func<Task> onCheckRequested)
    {
        return _docRef.Listen(async snapshot =>
        {
            if (snapshot.Exists &&
                snapshot.TryGetValue<bool>("check", out var check) &&
                check)
            {
                await onCheckRequested();
            }
        });
    }
}
