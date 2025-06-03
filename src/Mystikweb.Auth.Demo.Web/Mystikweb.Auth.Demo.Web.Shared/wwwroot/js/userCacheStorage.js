export function clearCachedUser() {
    localStorage.removeItem('cachedUser');
}

export function getCachedUser() {
    const cachedUser = localStorage.getItem('cachedUser');
    return !!cachedUser ? JSON.parse(cachedUser) : null;
}

export function isUserCached() {
    return localStorage.getItem('cachedUser') !== null;
}

export function setCachedUser(user) {
    if (user) {
        localStorage.setItem('cachedUser', JSON.stringify(user));
    } else {
        localStorage.removeItem('cachedUser');
    }
}