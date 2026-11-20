const anifilt = document.getElementById("af");
const linfo = document.getElementById("linfo");
const prs = document.getElementById("prs");
const prsval = document.getElementById("prsval");
const bsave = document.getElementById("bsave");
const bmake = document.getElementById("bmake");
const bklist = document.getElementById("bklist");
const autobk = document.getElementById("autobk");
const optm = document.getElementById("optm");
const crunkprio = document.getElementById("crunkprio");
const crunkprioval = document.getElementById("crunkprioval");
const crunkint = document.getElementById("crunkint");
const crunkintval = document.getElementById("crunkintval");
const bladd = document.getElementById("bladd");
const bladdbtn = document.getElementById("bladdbtn");
const bllist = document.getElementById("bllist");
const pathLauncher = document.getElementById("pathLauncher");
const pathJava = document.getElementById("pathJava");
const pathapply = document.getElementById("pathapply");
const profsel = document.getElementById("profsel");
const profcopy = document.getElementById("profcopy");
const preCombat = document.getElementById("preCombat");
const preOpt = document.getElementById("preOpt");
const preBeauty = document.getElementById("preBeauty");
const moptbtn = document.getElementById("moptbtn");
const optback = document.getElementById("optback");
const crunk = document.getElementById("crunk");
const kblist = document.getElementById("kblist");
const funl = document.getElementById("funl");
const fpsel = document.getElementById("fps");
const fpsval = document.getElementById("fpsval");
const twNewRender = document.getElementById("twNewRender");
const twRenderRegions = document.getElementById("twRenderRegions");
const twShowTips = document.getElementById("twShowTips");
const twSnooper = document.getElementById("twSnooper");
const twFreeze = document.getElementById("twFreeze");
const twBleed = document.getElementById("twBleed");
const twCameraShake = document.getElementById("twCameraShake");
const chColor = document.getElementById("chColor");
const chOutlineColor = document.getElementById("chOutlineColor");
const chStyle = document.getElementById("chStyle");
const rowCameraShake = document.getElementById("rowCameraShake");
const rowCrosshair = document.getElementById("rowCrosshair");
const rowSoundExtra = document.getElementById("rowSoundExtra");
const rowSndEntities = document.getElementById("rowSndEntities");
const rowSndExplosions = document.getElementById("rowSndExplosions");
const rowSndWeapons = document.getElementById("rowSndWeapons");
const profexp = document.getElementById("profexp");
const profimp = document.getElementById("profimp");
const slotname = document.getElementById("slotname");
const slotlist = document.getElementById("slotlist");
const kbsearch = document.getElementById("kbsearch");
const kbchanged = document.getElementById("kbchanged");
const standby = document.getElementById("standby");
const header = document.getElementById("header");
const imghh = document.querySelectorAll(".mgm");
const video = document.getElementById('selfsign');
const pend = new Map();
let opt = 0;
let nid = 1;
let lock = false;
let cap = null;
let transitioning = false;
let pendingTab = null;
let blCache = [];
let kbitems = [];
let kbbase = null;
let kbquery = "";
let kbchangedonly = false;
const fpsunl = 9999;
const pageDur = 100;
const pageShift = 12;
const maxBlur = 14;

imghh.forEach(el => {
    el.addEventListener("click", e => {
        header.classList.toggle("hidden");
    });
});

const slds = [
    { id: "rd", k: "render_distance", sc: "p", fmt: (v) => Math.round(v) + " чанков" },
    { id: "mip", k: "mipmap_levels", sc: "p", fmt: (v) => String(Math.round(v)) },
    { id: "fps", k: "fps_limit", sc: "p", fmt: (v) => Math.round(v) + " FPS" },
    { id: "fov", k: "fov", sc: "p", fmt: (v) => Math.round(v) + "°" },
    { id: "mem", k: "memory_gb", sc: "l", fmt: (v) => Number(v).toFixed(1) + " ГБ" }
];

const tgls = [
    { id: "vs", k: "v_sync", sc: "p" },
    { id: "sfps", k: "show_fps", sc: "p" },
    { id: "fs", k: "full_screen", sc: "l" },
    { id: "dbg", k: "debug", sc: "l" }
];

const twb = [
    { el: twNewRender, k: "newRender" },
    { el: twRenderRegions, k: "renderRegions" },
    { el: twShowTips, k: "showTips" },
    { el: twSnooper, k: "snooper" },
    { el: twFreeze, k: "freezingOverlay" },
    { el: twBleed, k: "bleedingOverlay" },
    { el: twCameraShake, k: "cameraShake" }
];

const twKeys = {
    twNewRender: "new_render",
    twRenderRegions: "render_regions",
    twShowTips: "show_tips",
    twSnooper: "snooper",
    twFreeze: "freezing_overlay",
    twBleed: "bleeding_overlay",
    twCameraShake: "camera_shake"
};

const chslds = [
    { id: "chOpacity", k: "opacity", fmt: (v) => Number(v).toFixed(2) },
    { id: "chGap", k: "gap", fmt: (v) => Number(v).toFixed(1) },
    { id: "chLen", k: "line_length", fmt: (v) => Number(v).toFixed(1) },
    { id: "chWidth", k: "line_width", fmt: (v) => Number(v).toFixed(1) },
    { id: "chDot", k: "dot_size", fmt: (v) => Number(v).toFixed(1) },
    { id: "chOutWidth", k: "outline_width", fmt: (v) => Number(v).toFixed(1) }
];

const chtgls = [
    { id: "chEnabled", k: "enabled", key: "enabled" },
    { id: "chOutline", k: "outline", key: "outline" },
    { id: "chDynSpacing", k: "dynamicSpacing", key: "dynamic_spacing" },
    { id: "chDynVis", k: "dynamicVisibility", key: "dynamic_visibility" }
];

const sndslds = [
    { id: "sndEntities", k: "entities_volume", row: rowSndEntities, has: "hasEntitiesVolume", val: "entitiesVolume", fmt: (v) => Math.round(Number(v) * 100) + "%" },
    { id: "sndExplosions", k: "explosions_volume", row: rowSndExplosions, has: "hasExplosionsVolume", val: "explosionsVolume", fmt: (v) => Math.round(Number(v) * 100) + "%" },
    { id: "sndWeapons", k: "weapons_volume", row: rowSndWeapons, has: "hasWeaponsVolume", val: "weaponsVolume", fmt: (v) => Math.round(Number(v) * 100) + "%" }
];

const prioNames = ["Обычный", "Выше обычного", "Высокий"];

const tabs = [
    { t: "tgraf", p: "pgraf" },
    { t: "tlaunch", p: "plaunch" },
    { t: "topt", p: "popt" },
    { t: "tctrl", p: "pctrl" },
    { t: "tabout", p: "pabout" },
    { t: "tset", p: "pset" }
];

const pnames = ["Низкие", "Средние", "Высокие"];
const pkeys = ["low", "medium", "high"];

const glfw = {
    KeyA: 65, KeyB: 66, KeyC: 67, KeyD: 68, KeyE: 69, KeyF: 70, KeyG: 71, KeyH: 72,
    KeyI: 73, KeyJ: 74, KeyK: 75, KeyL: 76, KeyM: 77, KeyN: 78, KeyO: 79, KeyP: 80,
    KeyQ: 81, KeyR: 82, KeyS: 83, KeyT: 84, KeyU: 85, KeyV: 86, KeyW: 87, KeyX: 88,
    KeyY: 89, KeyZ: 90,
    Digit0: 48, Digit1: 49, Digit2: 50, Digit3: 51, Digit4: 52,
    Digit5: 53, Digit6: 54, Digit7: 55, Digit8: 56, Digit9: 57,
    Space: 32, Quote: 39, Comma: 44, Minus: 45, Period: 46, Slash: 47,
    Semicolon: 59, Equal: 61, BracketLeft: 91, Backslash: 92, BracketRight: 93, Backquote: 96,
    Enter: 257, Tab: 258, Backspace: 259, Insert: 260, Delete: 261,
    ArrowRight: 262, ArrowLeft: 263, ArrowDown: 264, ArrowUp: 265,
    PageUp: 266, PageDown: 267, Home: 268, End: 269,
    CapsLock: 280, ScrollLock: 281, NumLock: 282, PrintScreen: 283, Pause: 284,
    F1: 290, F2: 291, F3: 292, F4: 293, F5: 294, F6: 295, F7: 296, F8: 297,
    F9: 298, F10: 299, F11: 300, F12: 301, F13: 302, F14: 303, F15: 304, F16: 305,
    F17: 306, F18: 307, F19: 308, F20: 309, F21: 310, F22: 311, F23: 312, F24: 313,
    Numpad0: 320, Numpad1: 321, Numpad2: 322, Numpad3: 323, Numpad4: 324,
    Numpad5: 325, Numpad6: 326, Numpad7: 327, Numpad8: 328, Numpad9: 329,
    NumpadDecimal: 330, NumpadDivide: 331, NumpadMultiply: 332, NumpadSubtract: 333,
    NumpadAdd: 334, NumpadEnter: 335, NumpadEqual: 336,
    ShiftLeft: 340, ControlLeft: 341, AltLeft: 342, MetaLeft: 343,
    ShiftRight: 344, ControlRight: 345, AltRight: 346, MetaRight: 347, ContextMenu: 348
};

const gname = {
    0: "Не назначено",
    "-100": "ЛКМ", "-99": "ПКМ", "-98": "СКМ", "-97": "Кнопка 4", "-96": "Кнопка 5",
    32: "Space", 39: "'", 44: ",", 45: "-", 46: ".", 47: "/",
    48: "0", 49: "1", 50: "2", 51: "3", 52: "4", 53: "5", 54: "6", 55: "7", 56: "8", 57: "9",
    59: ";", 61: "=",
    65: "A", 66: "B", 67: "C", 68: "D", 69: "E", 70: "F", 71: "G", 72: "H", 73: "I",
    74: "J", 75: "K", 76: "L", 77: "M", 78: "N", 79: "O", 80: "P", 81: "Q", 82: "R",
    83: "S", 84: "T", 85: "U", 86: "V", 87: "W", 88: "X", 89: "Y", 90: "Z",
    91: "[", 92: "\\", 93: "]", 96: "`", 161: "World 1", 162: "World 2",
    256: "Escape", 257: "Enter", 258: "Tab", 259: "Backspace", 260: "Insert", 261: "Delete",
    262: "Стрелка вправо", 263: "Стрелка влево", 264: "Стрелка вниз", 265: "Стрелка вверх",
    266: "Page Up", 267: "Page Down", 268: "Home", 269: "End",
    280: "Caps Lock", 281: "Scroll Lock", 282: "Num Lock", 283: "Print Screen", 284: "Pause",
    290: "F1", 291: "F2", 292: "F3", 293: "F4", 294: "F5", 295: "F6", 296: "F7", 297: "F8",
    298: "F9", 299: "F10", 300: "F11", 301: "F12", 302: "F13", 303: "F14", 304: "F15",
    305: "F16", 306: "F17", 307: "F18", 308: "F19", 309: "F20", 310: "F21", 311: "F22",
    312: "F23", 313: "F24",
    320: "Num 0", 321: "Num 1", 322: "Num 2", 323: "Num 3", 324: "Num 4",
    325: "Num 5", 326: "Num 6", 327: "Num 7", 328: "Num 8", 329: "Num 9",
    330: "Num .", 331: "Num /", 332: "Num *", 333: "Num -", 334: "Num +",
    335: "Num Enter", 336: "Num =",
    340: "Левый Shift", 341: "Левый Ctrl", 342: "Левый Alt", 343: "Левый Super",
    344: "Правый Shift", 345: "Правый Ctrl", 346: "Правый Alt", 347: "Правый Super",
    348: "Menu"
};

const kbnames = {
    key_action: "Действие",
    key_admin_menu: "Админ-меню",
    key_arena_weapon: "Аренное оружие",
    key_back: "Назад",
    key_chat: "Чат",
    key_command: "Команда",
    key_craft: "Крафт",
    key_fire_mode: "Режим огня",
    key_forward: "Вперёд",
    key_freelook: "Фрилук",
    key_hotbar1: "Слот 1",
    key_hotbar2: "Слот 2",
    key_hotbar3: "Слот 3",
    key_hotbar4: "Слот 4",
    key_hotbar5: "Слот 5",
    key_hotbar6: "Слот 6",
    key_inspect: "Осмотр оружия",
    key_inventory: "Инвентарь",
    key_inventory_hover: "Быстрое перетаскивание",
    key_jump: "Прыжок",
    key_left: "Влево",
    key_load_toolbar: "Загрузить панель",
    key_map: "Карта",
    key_pick_block: "Выбор блока",
    key_player_list: "Список игроков",
    key_primary_attack: "Основная атака",
    key_reload: "Перезарядка",
    key_right: "Вправо",
    key_save_toolbar: "Сохранить панель",
    key_scope_zoom_in: "Приблизить прицел",
    key_scope_zoom_out: "Отдалить прицел",
    key_screenshot: "Скриншот",
    key_secondary_attack: "Вторичная атака",
    key_smooth_camera: "Плавная камера",
    key_sneak: "Красться",
    key_spectator_outlines: "Контуры наблюдателя",
    key_sprint: "Бег",
    key_swap_hands: "Смена руки",
    key_toggle_item_light: "Фонарик оружия",
    key_toggle_perspective: "Смена перспективы",
    key_voice_chat: "Голосовой чат",
    key_zoom: "Зум"
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            video.currentTime = 0;
            video.play();
        } else {
            video.pause();
        }
    });
}, { threshold: 0 });

function cubicBezierFactory(x1, y1, x2, y2) {
    const ax = 1.0 - 3.0 * x2 + 3.0 * x1;
    const bx = 3.0 * x2 - 6.0 * x1;
    const cx = 3.0 * x1;
    const ay = 1.0 - 3.0 * y2 + 3.0 * y1;
    const by = 3.0 * y2 - 6.0 * y1;
    const cy = 3.0 * y1;

    function calcBezier(a, b, c, t) {
        return ((a * t + b) * t + c) * t;
    }

    function derivative(a, b, c, t) {
        return (3.0 * a * t + 2.0 * b) * t + c;
    }

    function solveCurveX(x, eps) {
        let t2 = x;
        for (let i = 0; i < 8; i++) {
            const x2 = calcBezier(ax, bx, cx, t2) - x;
            if (Math.abs(x2) < eps) {
                return t2;
            }
            const d = derivative(ax, bx, cx, t2);
            if (Math.abs(d) < 1e-6) {
                break;
            }
            t2 = t2 - x2 / d;
        }

        let t1 = 0.0;
        t2 = 1.0;
        let t = x;
        while (t1 < t2) {
            const x2 = calcBezier(ax, bx, cx, t);
            if (Math.abs(x2 - x) < eps) {
                return t;
            }
            if (x > x2) {
                t1 = t;
            } else {
                t2 = t;
            }
            t = (t1 + t2) * 0.5;
        }
        return t;
    }

    return function (x) {
        if (x <= 0) {
            return 0;
        }
        if (x >= 1) {
            return 1;
        }
        return calcBezier(ay, by, cy, solveCurveX(x, 1e-7));
    };
}

const pageEase = cubicBezierFactory(1 / 3, 2 / 3, 2 / 3, 1);

function runPageAnim(el, fromOp, fromTx, toOp, toTx, done) {
    const start = performance.now();

    function frame(now) {
        let t = (now - start) / pageDur;
        if (t >= 1) {
            t = 1;
        }
        const e = pageEase(t);
        const op = fromOp + (toOp - fromOp) * e;
        if (opt === 0) {
            el.style.filter = "blur(" + ((1 - op) * maxBlur) + "px)";
            el.style.transform = "translateX(" + (fromTx + (toTx - fromTx) * e) + "px)";
        }
        el.style.opacity = String(op);
        if (t < 1) {
            requestAnimationFrame(frame);
        } else {
            if (done) {
                done();
            }
        }
    }

    requestAnimationFrame(frame);
}

function pageOut(el, done) {
    runPageAnim(el, 1, 0, 0, pageShift, done);
}

function pageIn(el, done) {
    runPageAnim(el, 0, -pageShift, 1, 0, done);
}

function getCurrentPanel() {
    for (const c of tabs) {
        const p = document.getElementById(c.p);
        if (!p.classList.contains("hide")) {
            return p;
        }
    }
    return null;
}

function wb() {
    return window.chrome && window.chrome.webview;
}

function send(act, pl) {
    return new Promise((res, rej) => {
        if (!wb()) {
            rej(new Error("Открой через приложение"));
            return;
        }
        const id = nid++;
        pend.set(id, { res, rej });
        window.chrome.webview.postMessage(JSON.stringify({ id, action: act, payload: pl }));
    });
}

if (wb()) {
    window.chrome.webview.addEventListener("message", (e) => {
        const r = JSON.parse(e.data);
        const w = pend.get(r.id);
        if (!w) {
            return;
        }
        pend.delete(r.id);
        if (r.ok) {
            w.res(r.data);
        } else {
            w.rej(new Error(r.error));
        }
    });
}

function setstat(t) {
    document.querySelectorAll(".stat").forEach((el) => {
        el.textContent = t;
    });
}

function dis(v) {
    document.querySelectorAll("input, button, select").forEach((el) => {
        el.disabled = v;
    });
}

function sval(c, v) {
    document.getElementById(c.id + "val").textContent = c.fmt(v);
}

function clamp(v, a, b) {
    return Math.min(b, Math.max(a, v));
}

function totalram(st) {
    return Math.max(1, Math.floor(st.totalRamGb - 2));
}

function toHexColor(n) {
    const v = clamp(Math.round(Number(n) || 0), 0, 16777215);
    return "#" + v.toString(16).padStart(6, "0");
}

function fromHexColor(s) {
    const v = parseInt(String(s).replace("#", ""), 16);
    if (!Number.isFinite(v)) {
        return 0;
    }
    return clamp(v, 0, 16777215);
}

function renderprofiles(items, active) {
    profsel.textContent = "";
    const list = Array.isArray(items) ? items : [];
    list.forEach((it) => {
        const o = document.createElement("option");
        o.value = it.name;
        o.textContent = it.name;
        profsel.appendChild(o);
    });
    if (active) {
        profsel.value = active;
    }
}

function fill(st) {
    lock = true;

    autobk.checked = Boolean(st.autoBackup);
    standby.checked = Boolean(st.standbyClear);
    crunkprio.value = String(st.crunkPriority);
    crunkprioval.textContent = prioNames[st.crunkPriority] || String(st.crunkPriority);
    crunkint.value = String(Math.round(st.crunkIntervalMs / 1000));
    crunkintval.textContent = Math.round(st.crunkIntervalMs / 1000) + " с";
    blCache = Array.isArray(st.crunkBlacklist) ? st.crunkBlacklist.slice() : [];
    renderbl();
    pathLauncher.value = st.customLauncherDir || "";
    pathJava.value = st.customJavaPath || "";
    renderprofiles(st.profiles, st.activeProfile);

    if (!st.found) {
        setstat("RustMe не найден. Укажи папку лаунчера в настройках или запусти игру один раз.");
        dis(true);
        lock = false;
        return;
    }

    dis(false);
    linfo.textContent = "Профиль: " + st.account + " · " + st.activeProfile;

    anifilt.checked = st.profile.afLevel === 0;
    crunk.checked = Boolean(st.autoCrunk);
    funl.checked = st.profile.fpsLimit === fpsunl || st.profile.fpsLimit === 0;
    fpsel.disabled = funl.checked;

    const sv = {
        rd: st.profile.renderDistance,
        mip: st.profile.mipmapLevels,
        fps: st.profile.fpsLimit,
        fov: clamp(st.profile.fov, 70, 90),
        mem: st.launcher.memoryGb
    };

    document.getElementById("mem").max = String(totalram(st));

    slds.forEach((c) => {
        if (c.id === "fps" && funl.checked) {
            return;
        }
        const el = document.getElementById(c.id);
        el.value = String(sv[c.id]);
        sval(c, sv[c.id]);
    });

    if (funl.checked) {
        fpsval.textContent = "Без лимита";
    }

    const tv = {
        vs: st.profile.vSync,
        sfps: st.profile.showFps,
        fs: st.launcher.fullScreen,
        dbg: st.launcher.debug
    };

    tgls.forEach((c) => {
        document.getElementById(c.id).checked = Boolean(tv[c.id]);
    });

    twb.forEach((c) => {
        c.el.checked = Boolean(st.profile[c.k]);
    });

    rowCameraShake.style.display = st.profile.hasCameraShake ? "" : "none";
    rowCrosshair.style.display = st.profile.hasCrosshair ? "" : "none";

    if (st.profile.hasCrosshair) {
        const ch = st.profile.crosshair;
        chtgls.forEach((c) => {
            document.getElementById(c.id).checked = Boolean(ch[c.k]);
        });
        chColor.value = toHexColor(ch.color);
        chOutlineColor.value = toHexColor(ch.outlineColor);
        chStyle.textContent = ch.style || "—";
        const chv = {
            chOpacity: ch.opacity,
            chGap: ch.gap,
            chLen: ch.lineLength,
            chWidth: ch.lineWidth,
            chDot: ch.dotSize,
            chOutWidth: ch.outlineWidth
        };
        chslds.forEach((c) => {
            const el = document.getElementById(c.id);
            el.value = String(chv[c.id]);
            sval(c, chv[c.id]);
        });
    }

    let anysound = false;
    sndslds.forEach((c) => {
        const has = Boolean(st.profile[c.has]);
        c.row.style.display = has ? "" : "none";
        if (!has) {
            return;
        }
        anysound = true;
        const el = document.getElementById(c.id);
        el.value = String(st.profile[c.val]);
        sval(c, st.profile[c.val]);
    });
    rowSoundExtra.style.display = anysound ? "" : "none";

    renderkb(st.bindings);

    lock = false;

    if (st.running) {
        setstat("Лаунчер запущен. Закрой его перед изменением настроек.");
    } else {
        setstat("");
    }
}

async function pull() {
    try {
        const st = await send("getState", {});
        fill(st);
    } catch (err) {
        setstat(err.message);
    }
}

async function push(act, pl) {
    try {
        const st = await send(act, pl);
        fill(st);
    } catch (err) {
        setstat(err.message);
        await pull();
    }
}

async function bpush(act, pl) {
    try {
        await send(act, pl);
        await pull();
        await loadbackups();
        setstat("Готово");
    } catch (err) {
        setstat(err.message);
    }
}

async function slotpush(act, pl) {
    try {
        const st = await send(act, pl);
        fill(st);
        await loadslots();
        setstat("Готово");
    } catch (err) {
        setstat(err.message);
        await pull();
    }
}

function showtab(id) {
    if (transitioning) {
        pendingTab = id;
        return;
    }

    const target = tabs.find((c) => c.t === id);
    if (!target) {
        return;
    }

    const toEl = document.getElementById(target.p);
    const fromEl = getCurrentPanel();

    tabs.forEach((c) => {
        document.getElementById(c.t).classList.toggle("active", c.t === id);
    });

    if (fromEl === toEl) {
        return;
    }

    transitioning = true;

    pageOut(fromEl, () => {
        fromEl.classList.add("hide");
        fromEl.style.opacity = "";
        fromEl.style.transform = "";
        fromEl.style.filter = "";

        if (opt === 0) {
            toEl.style.transform = "translateX(" + -pageShift + "px)";
            toEl.style.filter = "blur(" + maxBlur + "px)";
        }
        toEl.style.opacity = "0";
        toEl.classList.remove("hide");
        void toEl.offsetWidth;

        pageIn(toEl, () => {
            if (opt === 0) {
                toEl.style.transform = "";
                toEl.style.filter = "";
            }
            toEl.style.opacity = "";
            transitioning = false;

            if (id === "tset") {
                loadbackups();
            }

            if (id === "topt") {
                loadslots();
            }

            if (pendingTab) {
                const next = pendingTab;
                pendingTab = null;
                showtab(next);
            }
        });
    });
}

function loadopt() {
    if (localStorage.getItem('opt') === "true") {
        document.body.classList.add("opt");
        opt = 1;
        optm.checked = true;
    }
}

function initlabels() {
    slds.forEach((c) => {
        const el = document.getElementById(c.id);
        sval(c, el.value);
    });
    chslds.forEach((c) => {
        const el = document.getElementById(c.id);
        sval(c, el.value);
    });
    sndslds.forEach((c) => {
        const el = document.getElementById(c.id);
        sval(c, el.value);
    });
    prsval.textContent = pnames[Number(prs.value)];
    crunkprioval.textContent = prioNames[Number(crunkprio.value)];
    crunkintval.textContent = Math.round(Number(crunkint.value)) + " с";
}

async function loadbackups() {
    try {
        const items = await send("listBackups", {});
        renderbackups(Array.isArray(items) ? items : []);
    } catch (err) {
        renderbackups([]);
    }
}

function renderbackups(items) {
    bklist.textContent = "";
    if (!items.length) {
        const e = document.createElement("div");
        e.className = "bkempty";
        e.textContent = "Бэкапов пока нет";
        bklist.appendChild(e);
        return;
    }
    items.forEach((it) => {
        const row = document.createElement("div");
        row.className = "bkitem";
        const info = document.createElement("div");
        info.className = "bkinfo";
        const nm = document.createElement("span");
        nm.className = "bkname";
        nm.textContent = it.name;
        const dt = document.createElement("span");
        dt.className = "bkdate";
        dt.textContent = it.created;
        info.appendChild(nm);
        info.appendChild(dt);
        const acts = document.createElement("div");
        acts.className = "bkacts";
        const rb = document.createElement("button");
        rb.className = "abtn mini";
        rb.textContent = "Восстановить";
        rb.addEventListener("click", () => {
            bpush("restoreBackup", { name: it.name });
        });
        const db = document.createElement("button");
        db.className = "abtn mini";
        db.textContent = "Удалить";
        db.addEventListener("click", () => {
            bpush("deleteBackup", { name: it.name });
        });
        acts.appendChild(rb);
        acts.appendChild(db);
        row.appendChild(info);
        row.appendChild(acts);
        bklist.appendChild(row);
    });
}

async function loadslots() {
    try {
        const items = await send("listProfileSlots", {});
        renderslots(Array.isArray(items) ? items : []);
    } catch (err) {
        renderslots([]);
    }
}

function renderslots(items) {
    slotlist.textContent = "";
    for (let n = 1; n <= 3; n++) {
        const it = items.find((s) => s.slot === n);
        const filled = Boolean(it && it.filled);
        const row = document.createElement("div");
        row.className = "bkitem";
        const info = document.createElement("div");
        info.className = "bkinfo";
        const nm = document.createElement("span");
        nm.className = "bkname";
        nm.textContent = filled ? (it.name || "Профиль " + n) : "Слот " + n + " — пусто";
        const dt = document.createElement("span");
        dt.className = "bkdate";
        dt.textContent = filled ? it.created : "";
        info.appendChild(nm);
        info.appendChild(dt);
        const acts = document.createElement("div");
        acts.className = "bkacts";
        const sb = document.createElement("button");
        sb.className = "abtn mini";
        sb.textContent = "Сохранить сюда";
        sb.addEventListener("click", () => {
            slotpush("saveProfileSlot", { slot: n, name: slotname.value.trim() });
        });
        const ab = document.createElement("button");
        ab.className = "abtn mini";
        ab.textContent = "Применить";
        ab.disabled = !filled;
        ab.addEventListener("click", () => {
            slotpush("applyProfileSlot", { slot: n });
        });
        const db = document.createElement("button");
        db.className = "abtn mini";
        db.textContent = "Удалить";
        db.disabled = !filled;
        db.addEventListener("click", () => {
            slotpush("deleteProfileSlot", { slot: n });
        });
        acts.appendChild(sb);
        acts.appendChild(ab);
        acts.appendChild(db);
        row.appendChild(info);
        row.appendChild(acts);
        slotlist.appendChild(row);
    }
}

function renderbl() {
    bllist.textContent = "";
    if (!blCache.length) {
        const e = document.createElement("div");
        e.className = "bkempty";
        e.textContent = "Список пуст";
        bllist.appendChild(e);
        return;
    }
    blCache.forEach((name, idx) => {
        const row = document.createElement("div");
        row.className = "bkitem";
        const info = document.createElement("div");
        info.className = "bkinfo";
        const nm = document.createElement("span");
        nm.className = "bkname";
        nm.textContent = name;
        info.appendChild(nm);
        const acts = document.createElement("div");
        acts.className = "bkacts";
        const db = document.createElement("button");
        db.className = "abtn mini";
        db.textContent = "Удалить";
        db.addEventListener("click", () => {
            blCache.splice(idx, 1);
            push("setCrunkBlacklist", { list: blCache });
        });
        acts.appendChild(db);
        row.appendChild(info);
        row.appendChild(acts);
        bllist.appendChild(row);
    });
}

function kblabel(it) {
    return kbnames[it.key] || it.key.replace("key_", "").replace(/_/g, " ");
}

function renderkb(items) {
    kbitems = Array.isArray(items) ? items.slice() : [];
    if (kbbase === null && kbitems.length > 0) {
        kbbase = new Map();
        kbitems.forEach((it) => {
            kbbase.set(it.key, it.code);
        });
    }
    drawkb();
}

function drawkb() {
    endcap();
    kblist.textContent = "";
    const items = kbitems.filter((it) => {
        if (kbchangedonly && kbbase !== null && kbbase.get(it.key) === it.code) {
            return false;
        }
        if (kbquery.length === 0) {
            return true;
        }
        const label = kblabel(it).toLowerCase();
        const raw = it.key.toLowerCase();
        const keyname = (gname[it.code] || String(it.code)).toLowerCase();
        return label.includes(kbquery) || raw.includes(kbquery) || keyname.includes(kbquery);
    });
    if (!items.length) {
        const e = document.createElement("div");
        e.className = "bkempty";
        e.textContent = kbitems.length ? "Ничего не найдено" : "Биндов нет";
        kblist.appendChild(e);
        return;
    }
    items.forEach((it) => {
        const row = document.createElement("div");
        row.className = "row";
        const p = document.createElement("p");
        p.textContent = kblabel(it);
        const b = document.createElement("button");
        b.className = "abtn mini";
        b.textContent = gname[it.code] || String(it.code);
        b.addEventListener("click", () => {
            if (lock) {
                return;
            }
            startcap(b, it.key);
        });
        row.appendChild(p);
        row.appendChild(b);
        kblist.appendChild(row);
    });
}

function startcap(btn, key) {
    if (cap) {
        endcap();
    }
    cap = { key, btn, prev: btn.textContent };
    btn.textContent = "Нажми клавишу";
    btn.classList.add("cap");
}

function endcap() {
    if (!cap) {
        return;
    }
    cap.btn.textContent = cap.prev;
    cap.btn.classList.remove("cap");
    cap = null;
}

anifilt.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setAf", { disabled: anifilt.checked });
});

slds.forEach((c) => {
    const el = document.getElementById(c.id);
    el.addEventListener("input", () => {
        sval(c, el.value);
    });
    el.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push(c.sc === "l" ? "setLauncher" : "setProfile", { key: c.k, value: Number(el.value) });
    });
});

tgls.forEach((c) => {
    const el = document.getElementById(c.id);
    el.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push(c.sc === "l" ? "setLauncher" : "setProfile", { key: c.k, value: el.checked });
    });
});

Object.keys(twKeys).forEach((id) => {
    const el = document.getElementById(id);
    el.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push("setTweakBool", { key: twKeys[id], value: el.checked });
    });
});

chslds.forEach((c) => {
    const el = document.getElementById(c.id);
    el.addEventListener("input", () => {
        sval(c, el.value);
    });
    el.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push("setCrosshair", { key: c.k, value: Number(el.value) });
    });
});

chtgls.forEach((c) => {
    const el = document.getElementById(c.id);
    el.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push("setCrosshair", { key: c.key, value: el.checked });
    });
});

chColor.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrosshair", { key: "color", value: fromHexColor(chColor.value) });
});

chOutlineColor.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrosshair", { key: "outline_color", value: fromHexColor(chOutlineColor.value) });
});

sndslds.forEach((c) => {
    const el = document.getElementById(c.id);
    el.addEventListener("input", () => {
        sval(c, el.value);
    });
    el.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push("setSoundVolume", { key: c.k, value: Number(el.value) });
    });
});

profsel.addEventListener("change", () => {
    if (lock) {
        return;
    }
    kbbase = null;
    push("setGameProfile", { name: profsel.value });
});

profcopy.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("copyProfileToAll", {});
});

funl.addEventListener("change", () => {
    if (lock) {
        return;
    }
    if (funl.checked) {
        fpsel.disabled = true;
        fpsval.textContent = "Без лимита";
        push("setProfile", { key: "fps_limit", value: fpsunl });
    } else {
        fpsel.disabled = false;
        push("setProfile", { key: "fps_limit", value: Number(fpsel.value) });
    }
});

prs.addEventListener("input", () => {
    prsval.textContent = pnames[Number(prs.value)];
});

prs.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("applyPreset", { preset: pkeys[Number(prs.value)] });
});

crunk.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setAutoCrunk", { enabled: crunk.checked });
});

standby.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setStandbyClear", { enabled: standby.checked });
});

autobk.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setAutoBackup", { enabled: autobk.checked });
});

optm.addEventListener("change", () => {
    if (lock) {
        return;
    }

    localStorage.setItem('opt', optm.checked.toString());

    if (optm.checked === true) {
        document.body.classList.add("opt");
        opt = 1;
    }
    else {
        document.body.classList.remove("opt");
        opt = 0;
    }
});

crunkprio.addEventListener("input", () => {
    crunkprioval.textContent = prioNames[Number(crunkprio.value)];
});

crunkprio.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrunkPriority", { priority: Number(crunkprio.value) });
});

crunkint.addEventListener("input", () => {
    crunkintval.textContent = Math.round(Number(crunkint.value)) + " с";
});

crunkint.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrunkInterval", { ms: Math.round(Number(crunkint.value)) * 1000 });
});

bladdbtn.addEventListener("click", () => {
    const name = bladd.value.trim();
    if (name.length === 0) {
        return;
    }
    if (blCache.some((n) => n.toLowerCase() === name.toLowerCase())) {
        bladd.value = "";
        return;
    }
    blCache.push(name);
    bladd.value = "";
    push("setCrunkBlacklist", { list: blCache });
});

pathapply.addEventListener("click", () => {
    push("setCustomPaths", { launcherDir: pathLauncher.value.trim(), javaPath: pathJava.value.trim() });
});

kbsearch.addEventListener("input", () => {
    kbquery = kbsearch.value.trim().toLowerCase();
    drawkb();
});

kbchanged.addEventListener("click", () => {
    kbchangedonly = !kbchangedonly;
    kbchanged.textContent = kbchangedonly ? "Все бинды" : "Изменённые";
    drawkb();
});

profexp.addEventListener("click", async () => {
    if (lock) {
        return;
    }
    try {
        const name = slotname.value.trim() || "rustme-profile";
        const pick = await send("pickExportPath", { name });
        if (!pick.path) {
            return;
        }
        const st = await send("exportProfile", { path: pick.path, name });
        fill(st);
        setstat("Профиль сохранён: " + pick.path);
    } catch (err) {
        setstat(err.message);
    }
});

profimp.addEventListener("click", async () => {
    if (lock) {
        return;
    }
    try {
        const pick = await send("pickImportPath", {});
        if (!pick.path) {
            return;
        }
        const st = await send("importProfile", { path: pick.path });
        fill(st);
        setstat("Профиль импортирован: " + pick.path);
    } catch (err) {
        setstat(err.message);
        await pull();
    }
});

preCombat.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("applyGeneralPreset", { preset: "combat" });
});

preOpt.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("applyGeneralPreset", { preset: "optimize" });
});

preBeauty.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("applyGeneralPreset", { preset: "beauty" });
});

moptbtn.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("megaoptimize", {});
});

optback.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("rollback", {});
});

bsave.addEventListener("click", () => {
    bpush("saveSettings", {});
});

bmake.addEventListener("click", () => {
    bpush("createBackup", {});
});

tabs.forEach((c) => {
    document.getElementById(c.t).addEventListener("click", () => {
        showtab(c.t);
    });
});

window.addEventListener("keydown", (e) => {
    if (!cap) {
        return;
    }
    e.preventDefault();
    if (e.repeat) {
        return;
    }
    if (e.code === "Escape") {
        endcap();
        return;
    }
    const code = glfw[e.code];
    if (code === undefined) {
        return;
    }
    const key = cap.key;
    endcap();
    push("setKeybind", { key, value: code });
});

window.addEventListener("mousedown", (e) => {
    if (!cap) {
        return;
    }
    e.preventDefault();
    const mb = { 0: -100, 1: -98, 2: -99, 3: -97, 4: -96 };
    const code = mb[e.button];
    if (code === undefined) {
        return;
    }
    const key = cap.key;
    endcap();
    push("setKeybind", { key, value: code });
});

observer.observe(video);
initlabels();
loadbackups();
loadslots();
loadopt();
pull();