// Mirrors Domain.User.UserRole (excluding Admin, which never fills out forms).
export const ROLE_LABELS: Record<string, string> = {
  StudentGroup: "Студенты",
  Staff: "Сотрудники",
  DeputyHead: "Заместитель декана",
  Employer: "Наниматели",
};
