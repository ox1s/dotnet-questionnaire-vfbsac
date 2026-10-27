import { describe, it, expect } from "vitest";
import { OWN_ROLE_LABELS, ROLE_LABELS } from "./roles";

describe("role labels", () => {
  it("labels teachers as a form audience and as the signed-in user's role", () => {
    expect(ROLE_LABELS.Teacher).toBe("Преподаватели");
    expect(OWN_ROLE_LABELS.Teacher).toBe("Преподаватель");
  });
});
