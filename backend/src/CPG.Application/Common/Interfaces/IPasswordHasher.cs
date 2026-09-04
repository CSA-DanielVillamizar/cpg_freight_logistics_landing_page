namespace CPG.Application.Common.Interfaces;

/// <summary>Hashes and verifies user passwords (SPEC.md US-01 - secure credential storage).</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
