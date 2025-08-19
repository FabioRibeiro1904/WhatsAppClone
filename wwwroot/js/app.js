window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};

window.autoResizeTextarea = (element) => {
    if (element) {
        element.style.height = 'auto';
        element.style.height = element.scrollHeight + 'px';
    }
};

window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy: ', err);
        return false;
    }
};

window.downloadFile = (filename, content, contentType) => {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
};

window.getElementPosition = (element) => {
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            top: rect.top,
            left: rect.left,
            width: rect.width,
            height: rect.height
        };
    }
    return null;
};

window.smoothScrollToElement = (element) => {
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'end' });
    }
};

window.isElementInViewport = (element) => {
    if (!element) return false;
    
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
};

window.initializeTooltips = () => {
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
};

window.playNotificationSound = () => {
    try {
        // Criar um som simples usando Web Audio API
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();
        
        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);
        
        oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.5);
        
        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.5);
    } catch (e) {
        console.log('Could not create notification sound:', e);
    }
};

window.logoutUser = async () => {
    try {
        console.log('JavaScript logoutUser chamado');
        
        const response = await fetch('/api/AuthApi/logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        console.log('Response status:', response.status);
        
        if (response.ok) {
            console.log('Logout bem-sucedido via API!');
            return true;
        } else {
            console.log('Logout falhou:', response.statusText);
            return false;
        }
    } catch (error) {
        console.error('Erro na chamada da API de logout:', error);
        return false;
    }
};

window.submitLoginForm = function(username, password) {
    console.log('submitLoginForm chamada com:', username, password);
    
    try {
        // Criar um form invisível para submit tradicional
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/api/AuthApi/login';
        form.style.display = 'none';
        
        // Adicionar campo username
        const usernameInput = document.createElement('input');
        usernameInput.type = 'hidden';
        usernameInput.name = 'Username';
        usernameInput.value = username;
        form.appendChild(usernameInput);
        
        // Adicionar campo password
        const passwordInput = document.createElement('input');
        passwordInput.type = 'hidden';
        passwordInput.name = 'Password';
        passwordInput.value = password;
        form.appendChild(passwordInput);
        
        // Adicionar ao DOM e submeter
        document.body.appendChild(form);
        console.log('Submetendo form para login...');
        form.submit();
        
        return true;
    } catch (error) {
        console.error('Erro na submitLoginForm:', error);
        return false;
    }
};

// Garantir que a função esteja disponível globalmente
globalThis.submitLoginForm = window.submitLoginForm;

// Debug - listar todas as funções disponíveis
console.log('Funções disponíveis no window:', Object.keys(window).filter(key => typeof window[key] === 'function'));
