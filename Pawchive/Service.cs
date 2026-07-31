namespace Pawchive;

public readonly record struct Service(string Name)
{
	public static readonly Service Patreon = new("patreon");
	public static readonly Service PixivFanbox = new("fanbox");

	public static implicit operator string(Service service) => service.Name;
	public static implicit operator Service(string name) => new(name);

	public override string ToString() => Name;
}