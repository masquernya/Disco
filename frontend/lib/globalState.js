
let _user = undefined;
let _setUser = undefined;
export function getUser() {
  return _user;
}
export function setUser(newUser) {
  _user = newUser;
  _setUser(newUser);
}
export function setUserFunctions(newUser, newSetUser) {
  _setUser = newSetUser;
  _user = newUser;
}