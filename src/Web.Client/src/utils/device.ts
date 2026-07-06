export const getDeviceId = (): string => {
  let deviceId = localStorage.getItem("deviceId");

  if (!deviceId) {
    deviceId = crypto.randomUUID
      ? crypto.randomUUID()
      : "device-" +
        Math.random().toString(36).substring(2) +
        Date.now().toString(36);

    localStorage.setItem("deviceId", deviceId);
  }

  return deviceId;
};
