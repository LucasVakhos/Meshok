using MeshokBrowser.Models;

namespace MeshokBrowser.Data;

public interface ILookupRepository
{
    KeyValuePair<int, string>[] BaseStatuses();
    KeyValuePair<int, string>[] SiteStatuses();
    KeyValuePair<int, string>[] DeliveryModes();
    List<User> LoadActiveUsers();
    List<MessagesSet> LoadMessageSettings();
    void SaveMessageSetting(MessagesSet item);
    void DeleteMessageSetting(int id);

    // Async versions
    Task<List<User>> LoadActiveUsersAsync();
    Task<List<MessagesSet>> LoadMessageSettingsAsync();
    Task SaveMessageSettingAsync(MessagesSet item);
}
