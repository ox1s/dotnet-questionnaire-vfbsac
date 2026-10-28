import { describe, it, expect, afterEach, vi } from "vitest";
import { getEnvLabel, withEnvLabel } from "./env-label";

describe("env label", () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it("is empty when VITE_ENV_LABEL is not set, as on production", () => {
    vi.stubEnv("VITE_ENV_LABEL", "");

    expect(getEnvLabel()).toBe("");
    expect(withEnvLabel("ВФБГАС — Анкетирование")).toBe(
      "ВФБГАС — Анкетирование",
    );
  });

  it("ignores a label made only of spaces", () => {
    vi.stubEnv("VITE_ENV_LABEL", "   ");

    expect(withEnvLabel("ВФБГАС")).toBe("ВФБГАС");
  });

  it("prefixes the title with the label on a test deploy", () => {
    vi.stubEnv("VITE_ENV_LABEL", " ТЕСТ ");

    expect(getEnvLabel()).toBe("ТЕСТ");
    expect(withEnvLabel("ВФБГАС — Анкетирование")).toBe(
      "[ТЕСТ] ВФБГАС — Анкетирование",
    );
  });
});
