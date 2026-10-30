namespace Perry.Api.Chat;

internal sealed record LocalChatUser(string Alias, Guid Id);

internal static class LocalChatUsers
{
    public static IReadOnlyList<LocalChatUser> All { get; } = Array.AsReadOnly(new[]
    {
        new LocalChatUser("alice", Guid.Parse("11111111-1111-1111-1111-111111111111")),
        new LocalChatUser("bob", Guid.Parse("22222222-2222-2222-2222-222222222222")),
        new LocalChatUser("charlie", Guid.Parse("33333333-3333-3333-3333-333333333333"))
    });

    public static bool Contains(Guid id) => All.Any(user => user.Id == id);
}
