const shot = "https://github.com/vexikoff/rmt/releases/download/bgprc/bgprc.png";

const gid = (id) => document.getElementById(id);
const af = gid("af");
const linfo = gid("linfo");
const prs = gid("prs");
const prsval = gid("prsval");
const bsave = gid("bsave");
const bmake = gid("bmake");
const bklist = gid("bklist");
const abk = gid("abk");
const optm = gid("optm");
const cprio = gid("cprio");
const cprioval = gid("cprioval");
const cint = gid("cint");
const cintval = gid("cintval");
const bladd = gid("bladd");
const blbtn = gid("blbtn");
const blist = gid("blist");
const dpath = gid("dpath");
const jpath = gid("jpath");
const papply = gid("papply");
const psel = gid("psel");
const praw = gid("praw");
const pcopy = gid("pcopy");
const bcom = gid("bcom");
const bopt = gid("bopt");
const bbea = gid("bbea");
const mopt = gid("mopt");
const oback = gid("oback");
const crunk = gid("crunk");
const kbl = gid("kbl");
const funl = gid("funl");
const fps = gid("fps");
const fpsval = gid("fpsval");
const nrender = gid("nrender");
const rreg = gid("rreg");
const tips = gid("tips");
const snoop = gid("snoop");
const freeze = gid("freeze");
const bleed = gid("bleed");
const shake = gid("shake");
const xcolor = gid("xcolor");
const xoutcolor = gid("xoutcolor");
const xstyle = gid("xstyle");
const xnone = gid("xnone");
const ximg = gid("ximg");
const xcan = gid("xcan");
const xop = gid("xop");
const xgap = gid("xgap");
const xlen = gid("xlen");
const xwid = gid("xwid");
const xdot = gid("xdot");
const xout = gid("xout");
const xow = gid("xow");
const rshake = gid("rshake");
const rcross = gid("rcross");
const pexp = gid("pexp");
const pimp = gid("pimp");
const sname = gid("sname");
const slist = gid("slist");
const kbs = gid("kbs");
const kbc = gid("kbc");
const stby = gid("stby");
const hdr = gid("hdr");
const mgm = document.querySelectorAll(".mgm");
const sign = gid("sign");
const pend = new Map();
let opt = 0;
let nid = 1;
let lock = false;
let cap = null;
let busy = false;
let next = null;
let cur = null;
let blcache = [];
let kbitems = [];
let kbbase = null;
let kbq = "";
let kbmod = false;
const fpsunl = 9999;
const dur = 100;
const shift = 12;
const maxblur = 14;

mgm.forEach((node) => {
    node.addEventListener("click", () => {
        hdr.classList.toggle("hidden");
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
    { node: nrender, k: "newRender" },
    { node: rreg, k: "renderRegions" },
    { node: tips, k: "showTips" },
    { node: snoop, k: "snooper" },
    { node: freeze, k: "freezingOverlay" },
    { node: bleed, k: "bleedingOverlay" },
    { node: shake, k: "cameraShake" }
];

const twk = {
    nrender: "new_render",
    rreg: "render_regions",
    tips: "show_tips",
    snoop: "snooper",
    freeze: "freezing_overlay",
    bleed: "bleeding_overlay",
    shake: "camera_shake"
};

const chs = [
    { id: "xop", k: "opacity", fmt: (v) => Number(v).toFixed(2) },
    { id: "xgap", k: "gap", fmt: (v) => Number(v).toFixed(1) },
    { id: "xlen", k: "line_length", fmt: (v) => Number(v).toFixed(1) },
    { id: "xwid", k: "line_width", fmt: (v) => Number(v).toFixed(1) },
    { id: "xdot", k: "dot_size", fmt: (v) => Number(v).toFixed(1) },
    { id: "xow", k: "outline_width", fmt: (v) => Number(v).toFixed(1) }
];

const cht = [
    { id: "xout", k: "outline", key: "outline" },
    { id: "xdyn", k: "dynamicSpacing", key: "dynamic_spacing" },
    { id: "xvis", k: "dynamicVisibility", key: "dynamic_visibility" }
];

const prion = ["Обычный", "Выше обычного", "Высокий"];

const tabs = [
    { t: "tgraf", p: "pgraf" },
    { t: "tcross", p: "pcross" },
    { t: "tlch", p: "plch" },
    { t: "topt", p: "popt" },
    { t: "tkey", p: "pkey" },
    { t: "tabt", p: "pabt" },
    { t: "tset", p: "pset" }
];

const pnames = ["Низкие", "Средние", "Высокие"];
const pkeys = ["low", "medium", "high"];

const profknown = {
    "prod-a": "Основной",
    "prod--b": "Запасной",
    "testing": "Тестовый"
};

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

const kbn = {
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

const obs = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            sign.currentTime = 0;
            sign.play();
        } else {
            sign.pause();
        }
    });
}, { threshold: 0 });

function ease(x1, y1, x2, y2) {
    const ax = 1.0 - 3.0 * x2 + 3.0 * x1;
    const bx = 3.0 * x2 - 6.0 * x1;
    const cx = 3.0 * x1;
    const ay = 1.0 - 3.0 * y2 + 3.0 * y1;
    const by = 3.0 * y2 - 6.0 * y1;
    const cy = 3.0 * y1;

    function calc(a, b, c, t) {
        return ((a * t + b) * t + c) * t;
    }

    function deriv(a, b, c, t) {
        return (3.0 * a * t + 2.0 * b) * t + c;
    }

    function solve(x, eps) {
        let t2 = x;
        for (let i = 0; i < 8; i++) {
            const d = calc(ax, bx, cx, t2) - x;
            if (Math.abs(d) < eps) {
                return t2;
            }
            const dd = deriv(ax, bx, cx, t2);
            if (Math.abs(dd) < 1e-6) {
                break;
            }
            t2 = t2 - d / dd;
        }

        let t1 = 0.0;
        t2 = 1.0;
        let t = x;
        while (t1 < t2) {
            const d = calc(ax, bx, cx, t);
            if (Math.abs(d - x) < eps) {
                return t;
            }
            if (x > d) {
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
        return calc(ay, by, cy, solve(x, 1e-7));
    };
}

const pease = ease(1 / 3, 2 / 3, 2 / 3, 1);

function anim(node, fo, ft, to, tt, done) {
    const start = performance.now();

    function frame(now) {
        let t = (now - start) / dur;
        if (t >= 1) {
            t = 1;
        }
        const e = pease(t);
        const op = fo + (to - fo) * e;
        if (opt === 0) {
            node.style.filter = "blur(" + ((1 - op) * maxblur) + "px)";
            node.style.transform = "translateX(" + (ft + (tt - ft) * e) + "px)";
        }
        node.style.opacity = String(op);
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

function pout(node, done) {
    anim(node, 1, 0, 0, shift, done);
}

function pin(node, done) {
    anim(node, 0, -shift, 1, 0, done);
}

function curpan() {
    for (const c of tabs) {
        const p = gid(c.p);
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

function stat(t) {
    document.querySelectorAll(".stat").forEach((node) => {
        node.textContent = t;
    });
}

function dis(v) {
    document.querySelectorAll("input, button, select").forEach((node) => {
        node.disabled = v;
    });
}

function sval(c, v) {
    gid(c.id + "val").textContent = c.fmt(v);
}

function clamp(v, a, b) {
    return Math.min(b, Math.max(a, v));
}

function ram(st) {
    return Math.max(1, Math.floor(st.totalRamGb - 2));
}

function hex(n) {
    const v = clamp(Math.round(Number(n) || 0), 0, 16777215);
    return "#" + v.toString(16).padStart(6, "0");
}

function unhex(s) {
    const v = parseInt(String(s).replace("#", ""), 16);
    if (!Number.isFinite(v)) {
        return 0;
    }
    return clamp(v, 0, 16777215);
}

function plabel(name, idx) {
    return profknown[name] || "Профиль " + (idx + 1);
}

function pfill(items, active) {
    psel.textContent = "";
    const list = Array.isArray(items) ? items : [];
    list.forEach((it, idx) => {
        const o = document.createElement("option");
        o.value = it.name;
        o.textContent = plabel(it.name, idx);
        psel.appendChild(o);
    });
    if (active) {
        psel.value = active;
    }
    praw.textContent = active ? "Папка: " + active : "";
}

if (shot) {
    ximg.src = shot;
} else {
    ximg.style.display = "none";
}
ximg.addEventListener("error", () => {
    ximg.style.display = "none";
});

function drawx() {
    const box = xcan.parentElement.getBoundingClientRect();
    if (box.width < 10 || box.height < 10) {
        return;
    }
    const dpr = window.devicePixelRatio || 1;
    xcan.width = Math.round(box.width * dpr);
    xcan.height = Math.round(box.height * dpr);
    const ctx = xcan.getContext("2d");
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.clearRect(0, 0, box.width, box.height);
    if (!cur || !cur.found || !cur.profile.hasCrosshair) {
        return;
    }
    const cx = box.width / 2;
    const cy = box.height / 2;
    const sc = 4;
    const op = Number(xop.value);
    const gap = Number(xgap.value) * sc;
    const len = Number(xlen.value) * sc;
    const wid = Math.max(1, Number(xwid.value) * sc);
    const dot = Number(xdot.value) * sc;
    const out = xout.checked;
    const ow = Number(xow.value) * sc;
    const style = cur.profile.crosshair.style || "Dot";
    ctx.globalAlpha = op;
    function bar(x, y, w, h) {
        if (out) {
            ctx.fillStyle = xoutcolor.value;
            ctx.fillRect(x - ow, y - ow, w + ow * 2, h + ow * 2);
        }
        ctx.fillStyle = xcolor.value;
        ctx.fillRect(x, y, w, h);
    }
    if (style.toLowerCase() !== "dot") {
        bar(cx - wid / 2, cy - gap - len, wid, len);
        bar(cx - wid / 2, cy + gap, wid, len);
        bar(cx - gap - len, cy - wid / 2, len, wid);
        bar(cx + gap, cy - wid / 2, len, wid);
    }
    bar(cx - dot / 2, cy - dot / 2, dot, dot);
    ctx.globalAlpha = 1;
}

function fill(st) {
    lock = true;
    cur = st;

    abk.checked = Boolean(st.autoBackup);
    stby.checked = Boolean(st.standbyClear);
    cprio.value = String(st.crunkPriority);
    cprioval.textContent = prion[st.crunkPriority] || String(st.crunkPriority);
    cint.value = String(Math.round(st.crunkIntervalMs / 1000));
    cintval.textContent = Math.round(st.crunkIntervalMs / 1000) + " с";
    blcache = Array.isArray(st.crunkBlacklist) ? st.crunkBlacklist.slice() : [];
    drawbl();
    dpath.value = st.customLauncherDir || "";
    jpath.value = st.customJavaPath || "";
    pfill(st.profiles, st.activeProfile);

    if (!st.found) {
        stat("RustMe не найден. Укажи папку лаунчера в настройках или запусти игру один раз.");
        rcross.style.display = "none";
        xnone.style.display = "";
        xnone.textContent = "RustMe не найден. Настройки прицела появятся, когда твикер увидит лаунчер.";
        dis(true);
        drawx();
        lock = false;
        return;
    }

    dis(false);
    linfo.textContent = "Профиль: " + st.account + " · " + plabel(st.activeProfile, st.profiles.findIndex((p) => p.name === st.activeProfile));

    af.checked = st.profile.afLevel === 0;
    crunk.checked = Boolean(st.autoCrunk);
    funl.checked = st.profile.fpsLimit === fpsunl || st.profile.fpsLimit === 0;
    fps.disabled = funl.checked;

    const sv = {
        rd: st.profile.renderDistance,
        mip: st.profile.mipmapLevels,
        fps: st.profile.fpsLimit,
        fov: clamp(st.profile.fov, 70, 90),
        mem: st.launcher.memoryGb
    };

    gid("mem").max = String(ram(st));

    slds.forEach((c) => {
        if (c.id === "fps" && funl.checked) {
            return;
        }
        const node = gid(c.id);
        node.value = String(sv[c.id]);
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
        gid(c.id).checked = Boolean(tv[c.id]);
    });

    twb.forEach((c) => {
        c.node.checked = Boolean(st.profile[c.k]);
    });

    rshake.style.display = st.profile.hasCameraShake ? "" : "none";
    rcross.style.display = st.profile.hasCrosshair ? "" : "none";
    xnone.style.display = st.profile.hasCrosshair ? "none" : "";
    xnone.textContent = "В этом профиле игра не даёт настроек прицела. Выбери профиль, где они есть.";

    if (st.profile.hasCrosshair) {
        const ch = st.profile.crosshair;
        cht.forEach((c) => {
            gid(c.id).checked = Boolean(ch[c.k]);
        });
        xcolor.value = hex(ch.color);
        xoutcolor.value = hex(ch.outlineColor);
        xstyle.textContent = ch.style || "—";
        const chv = {
            xop: ch.opacity,
            xgap: ch.gap,
            xlen: ch.lineLength,
            xwid: ch.lineWidth,
            xdot: ch.dotSize,
            xow: ch.outlineWidth
        };
        chs.forEach((c) => {
            const node = gid(c.id);
            node.value = String(chv[c.id]);
            sval(c, chv[c.id]);
        });
    }

    drawkbitems(st.bindings);
    drawx();
    lock = false;

    if (st.running) {
        stat("Лаунчер запущен. Закрой его перед изменением настроек.");
    } else {
        stat("");
    }
}

async function pull() {
    try {
        const st = await send("getState", {});
        fill(st);
    } catch (err) {
        stat(err.message);
    }
}

async function push(act, pl) {
    try {
        const st = await send(act, pl);
        fill(st);
    } catch (err) {
        stat(err.message);
        await pull();
    }
}

async function bpush(act, pl) {
    try {
        await send(act, pl);
        await pull();
        await loadbk();
        stat("Готово");
    } catch (err) {
        stat(err.message);
    }
}

async function spush(act, pl) {
    try {
        const st = await send(act, pl);
        fill(st);
        await loadsl();
        stat("Готово");
    } catch (err) {
        stat(err.message);
        await pull();
    }
}

function show(id) {
    if (busy) {
        next = id;
        return;
    }

    const target = tabs.find((c) => c.t === id);
    if (!target) {
        return;
    }

    const to = gid(target.p);
    const from = curpan();

    tabs.forEach((c) => {
        gid(c.t).classList.toggle("active", c.t === id);
    });

    if (from === to) {
        return;
    }

    busy = true;

    pout(from, () => {
        from.classList.add("hide");
        from.style.opacity = "";
        from.style.transform = "";
        from.style.filter = "";

        if (opt === 0) {
            to.style.transform = "translateX(" + -shift + "px)";
            to.style.filter = "blur(" + maxblur + "px)";
        }
        to.style.opacity = "0";
        to.classList.remove("hide");
        void to.offsetWidth;

        pin(to, () => {
            if (opt === 0) {
                to.style.transform = "";
                to.style.filter = "";
            }
            to.style.opacity = "";
            busy = false;

            if (id === "tset") {
                loadbk();
            }

            if (id === "topt") {
                loadsl();
            }

            if (id === "tcross") {
                drawx();
            }

            if (next) {
                const n = next;
                next = null;
                show(n);
            }
        });
    });
}

function loadopt() {
    if (localStorage.getItem("opt") === "true") {
        document.body.classList.add("opt");
        opt = 1;
        optm.checked = true;
    }
}

function labels() {
    slds.forEach((c) => {
        sval(c, gid(c.id).value);
    });
    chs.forEach((c) => {
        sval(c, gid(c.id).value);
    });
    prsval.textContent = pnames[Number(prs.value)];
    cprioval.textContent = prion[Number(cprio.value)];
    cintval.textContent = Math.round(Number(cint.value)) + " с";
}

async function loadbk() {
    try {
        const items = await send("listBackups", {});
        drawbk(Array.isArray(items) ? items : []);
    } catch (err) {
        drawbk([]);
    }
}

function drawbk(items) {
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
        nm.textContent = "Копия от " + it.created;
        const dt = document.createElement("span");
        dt.className = "bkdate";
        dt.textContent = it.profile ? "Профиль: " + plabel(it.profile, 0) : it.created;
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

async function loadsl() {
    try {
        const items = await send("listProfileSlots", {});
        drawsl(Array.isArray(items) ? items : []);
    } catch (err) {
        drawsl([]);
    }
}

function drawsl(items) {
    slist.textContent = "";
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
            spush("saveProfileSlot", { slot: n, name: sname.value.trim() });
        });
        const ab = document.createElement("button");
        ab.className = "abtn mini";
        ab.textContent = "Применить";
        ab.disabled = !filled;
        ab.addEventListener("click", () => {
            spush("applyProfileSlot", { slot: n });
        });
        const db = document.createElement("button");
        db.className = "abtn mini";
        db.textContent = "Удалить";
        db.disabled = !filled;
        db.addEventListener("click", () => {
            spush("deleteProfileSlot", { slot: n });
        });
        acts.appendChild(sb);
        acts.appendChild(ab);
        acts.appendChild(db);
        row.appendChild(info);
        row.appendChild(acts);
        slist.appendChild(row);
    }
}

function drawbl() {
    blist.textContent = "";
    if (!blcache.length) {
        const e = document.createElement("div");
        e.className = "bkempty";
        e.textContent = "Список пуст";
        blist.appendChild(e);
        return;
    }
    blcache.forEach((name, idx) => {
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
            blcache.splice(idx, 1);
            push("setCrunkBlacklist", { list: blcache });
        });
        acts.appendChild(db);
        row.appendChild(info);
        row.appendChild(acts);
        blist.appendChild(row);
    });
}

function kblabel(it) {
    return kbn[it.key] || it.key.replace("key_", "").replace(/_/g, " ");
}

function drawkbitems(items) {
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
    capoff();
    kbl.textContent = "";
    const items = kbitems.filter((it) => {
        if (kbmod && kbbase !== null && kbbase.get(it.key) === it.code) {
            return false;
        }
        if (kbq.length === 0) {
            return true;
        }
        const label = kblabel(it).toLowerCase();
        const raw = it.key.toLowerCase();
        const keyname = (gname[it.code] || String(it.code)).toLowerCase();
        return label.includes(kbq) || raw.includes(kbq) || keyname.includes(kbq);
    });
    if (!items.length) {
        const e = document.createElement("div");
        e.className = "bkempty";
        e.textContent = kbitems.length ? "Ничего не найдено" : "Биндов нет";
        kbl.appendChild(e);
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
            capon(b, it.key);
        });
        row.appendChild(p);
        row.appendChild(b);
        kbl.appendChild(row);
    });
}

function capon(btn, key) {
    if (cap) {
        capoff();
    }
    cap = { key, btn, prev: btn.textContent };
    btn.textContent = "Нажми клавишу";
    btn.classList.add("cap");
}

function capoff() {
    if (!cap) {
        return;
    }
    cap.btn.textContent = cap.prev;
    cap.btn.classList.remove("cap");
    cap = null;
}

af.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setAf", { disabled: af.checked });
});

slds.forEach((c) => {
    const node = gid(c.id);
    node.addEventListener("input", () => {
        sval(c, node.value);
    });
    node.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push(c.sc === "l" ? "setLauncher" : "setProfile", { key: c.k, value: Number(node.value) });
    });
});

tgls.forEach((c) => {
    const node = gid(c.id);
    node.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push(c.sc === "l" ? "setLauncher" : "setProfile", { key: c.k, value: node.checked });
    });
});

Object.keys(twk).forEach((id) => {
    gid(id).addEventListener("change", () => {
        if (lock) {
            return;
        }
        push("setTweakBool", { key: twk[id], value: gid(id).checked });
    });
});

chs.forEach((c) => {
    const node = gid(c.id);
    node.addEventListener("input", () => {
        sval(c, node.value);
        drawx();
    });
    node.addEventListener("change", () => {
        if (lock) {
            return;
        }
        push("setCrosshair", { key: c.k, value: Number(node.value) });
    });
});

cht.forEach((c) => {
    const node = gid(c.id);
    node.addEventListener("change", () => {
        drawx();
        if (lock) {
            return;
        }
        push("setCrosshair", { key: c.key, value: node.checked });
    });
});

xcolor.addEventListener("input", () => {
    drawx();
});

xcolor.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrosshair", { key: "color", value: unhex(xcolor.value) });
});

xoutcolor.addEventListener("input", () => {
    drawx();
});

xoutcolor.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrosshair", { key: "outline_color", value: unhex(xoutcolor.value) });
});

psel.addEventListener("change", () => {
    if (lock) {
        return;
    }
    kbbase = null;
    push("setGameProfile", { name: psel.value });
});

pcopy.addEventListener("click", () => {
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
        fps.disabled = true;
        fpsval.textContent = "Без лимита";
        push("setProfile", { key: "fps_limit", value: fpsunl });
    } else {
        fps.disabled = false;
        push("setProfile", { key: "fps_limit", value: Number(fps.value) });
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

stby.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setStandbyClear", { enabled: stby.checked });
});

abk.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setAutoBackup", { enabled: abk.checked });
});

optm.addEventListener("change", () => {
    if (lock) {
        return;
    }

    localStorage.setItem("opt", optm.checked.toString());

    if (optm.checked === true) {
        document.body.classList.add("opt");
        opt = 1;
    }
    else {
        document.body.classList.remove("opt");
        opt = 0;
    }
});

cprio.addEventListener("input", () => {
    cprioval.textContent = prion[Number(cprio.value)];
});

cprio.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrunkPriority", { priority: Number(cprio.value) });
});

cint.addEventListener("input", () => {
    cintval.textContent = Math.round(Number(cint.value)) + " с";
});

cint.addEventListener("change", () => {
    if (lock) {
        return;
    }
    push("setCrunkInterval", { ms: Math.round(Number(cint.value)) * 1000 });
});

blbtn.addEventListener("click", () => {
    const name = bladd.value.trim();
    if (name.length === 0) {
        return;
    }
    if (blcache.some((n) => n.toLowerCase() === name.toLowerCase())) {
        bladd.value = "";
        return;
    }
    blcache.push(name);
    bladd.value = "";
    push("setCrunkBlacklist", { list: blcache });
});

papply.addEventListener("click", () => {
    push("setCustomPaths", { launcherDir: dpath.value.trim(), javaPath: jpath.value.trim() });
});

kbs.addEventListener("input", () => {
    kbq = kbs.value.trim().toLowerCase();
    drawkb();
});

kbc.addEventListener("click", () => {
    kbmod = !kbmod;
    kbc.textContent = kbmod ? "Все бинды" : "Изменённые";
    drawkb();
});

pexp.addEventListener("click", async () => {
    if (lock) {
        return;
    }
    try {
        const name = sname.value.trim() || "rustme-profile";
        const pick = await send("pickExportPath", { name });
        if (!pick.path) {
            return;
        }
        const st = await send("exportProfile", { path: pick.path, name });
        fill(st);
        stat("Профиль сохранён: " + pick.path);
    } catch (err) {
        stat(err.message);
    }
});

pimp.addEventListener("click", async () => {
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
        stat("Профиль импортирован: " + pick.path);
    } catch (err) {
        stat(err.message);
        await pull();
    }
});

bcom.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("applyGeneralPreset", { preset: "combat" });
});

bopt.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("applyGeneralPreset", { preset: "optimize" });
});

bbea.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("applyGeneralPreset", { preset: "beauty" });
});

mopt.addEventListener("click", () => {
    if (lock) {
        return;
    }
    push("megaoptimize", {});
});

oback.addEventListener("click", () => {
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
    gid(c.t).addEventListener("click", () => {
        show(c.t);
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
        capoff();
        return;
    }
    const code = glfw[e.code];
    if (code === undefined) {
        return;
    }
    const key = cap.key;
    capoff();
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
    capoff();
    push("setKeybind", { key, value: code });
});

window.addEventListener("resize", drawx);

obs.observe(sign);
labels();
loadbk();
loadsl();
loadopt();
pull();