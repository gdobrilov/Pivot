namespace RubiksCube.Application.Messaging;

/// <summary>Reads state, no side effects. One handler per query.</summary>
public interface IQuery<TResult>;
