const rangeInputs = document.querySelectorAll('input[type="range"]');
const animationDuration = 100;
const rendererStates = new Map();

function finiteNumber(value, fallback) {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) {
        return parsed;
    }
    return fallback;
}

function createRendererState(input) {
    const wrap = input.parentElement;
    if (!wrap) {
        return null;
    }

    if (input.dataset.jsrSlider === "1") {
        return null;
    }

    input.dataset.jsrSlider = "1";

    wrap.style.position = "relative";

    const track = document.createElement("div");
    track.style.position = "relative";
    track.style.display = "block";
    track.style.flex = "1";
    track.style.height = "28px";
    track.style.minWidth = "0";
    track.style.order = "2";
    track.style.background = "rgba(0, 0, 0, 0.31)";
    track.style.borderRadius = "25px";
    track.style.boxSizing = "border-box";
    track.style.overflow = "visible";

    const thumb = document.createElement("div");
    thumb.style.position = "absolute";
    thumb.style.top = "0";
    thumb.style.left = "0";
    thumb.style.width = "64px";
    thumb.style.height = "28px";
    thumb.style.boxSizing = "border-box";
    thumb.style.borderRadius = "25px";
    thumb.style.background = "#000000";
    thumb.style.border = "4px solid #FF3300";
    thumb.style.pointerEvents = "none";
    thumb.style.willChange = "transform";

    track.appendChild(thumb);

    wrap.insertBefore(track, input);
    track.appendChild(input);

    input.style.position = "absolute";
    input.style.top = "0";
    input.style.left = "0";
    input.style.width = "100%";
    input.style.height = "100%";
    input.style.margin = "0";
    input.style.padding = "0";
    input.style.border = "none";
    input.style.outline = "none";
    input.style.background = "transparent";
    input.style.opacity = "0";
    input.style.appearance = "none";
    input.style.webkitAppearance = "none";
    input.style.cursor = "default";
    input.style.zIndex = "2";

    const initialValue = finiteNumber(input.value, 0);

    const state = {
        input: input,
        track: track,
        thumb: thumb,
        current: initialValue,
        target: initialValue,
        from: initialValue,
        startTime: 0,
        animating: false
    };

    input.addEventListener("input", () => {
        startAnimation(state, finiteNumber(input.value, state.target));
    });

    input.addEventListener("change", () => {
        startAnimation(state, finiteNumber(input.value, state.target));
    });

    return state;
}

function startAnimation(state, target) {
    if (target === state.target) {
        return;
    }

    state.from = state.current;
    state.target = target;
    state.startTime = performance.now();
    state.animating = true;
}

function easeOutQuad(time) {
    return time * (2 - time);
}

function layoutThumb(state) {
    const min = finiteNumber(state.input.min, 0);
    const max = finiteNumber(state.input.max, 100);
    const range = max - min;

    const trackWidth = state.track.clientWidth;
    const thumbWidth = state.thumb.offsetWidth || 64;
    const usableWidth = Math.max(0, trackWidth - thumbWidth);

    let progress = 0;

    if (range > 0) {
        progress = (state.current - min) / range;
    }

    if (progress < 0) {
        progress = 0;
    }

    if (progress > 1) {
        progress = 1;
    }

    const offset = progress * usableWidth;
    state.thumb.style.transform = "translateX(" + offset + "px)";
}

function updateDisabledState(state) {
    if (state.input.disabled) {
        state.track.style.opacity = "0.4";
    } else {
        state.track.style.opacity = "1";
    }
}

function renderFrame(now) {
    rendererStates.forEach((state) => {
        const inputValue = finiteNumber(state.input.value, state.target);

        if (inputValue !== state.target) {
            startAnimation(state, inputValue);
        }

        if (state.animating) {
            const elapsed = now - state.startTime;
            const time = elapsed / animationDuration;

            if (time >= 1) {
                state.current = state.target;
                state.animating = false;
            } else {
                const eased = easeOutQuad(time);
                state.current = state.from + (state.target - state.from) * eased;
            }
        } else if (state.current !== inputValue) {
            state.current = inputValue;
        }

        updateDisabledState(state);
        layoutThumb(state);
    });

    requestAnimationFrame(renderFrame);
}

rangeInputs.forEach((input) => {
    const state = createRendererState(input);

    if (state) {
        rendererStates.set(input, state);
        layoutThumb(state);
    }
});

requestAnimationFrame(renderFrame);