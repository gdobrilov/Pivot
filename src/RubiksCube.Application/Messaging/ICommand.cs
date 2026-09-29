namespace RubiksCube.Application.Messaging;

/// <summary>Changes state. One handler per command.</summary>
public interface ICommand<TResult>;
