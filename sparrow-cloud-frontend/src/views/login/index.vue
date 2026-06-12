<template>
    <div class="login-page">
        <div class="login-card">
            <h2 class="title">登录</h2>

            <form @submit.prevent="onSubmit" class="form">
                <div class="field">
                    <label>用户名</label>
                    <input v-model="username" type="text" placeholder="请输入用户名" />
                </div>

                <div class="field">
                    <label>密码</label>
                    <input v-model="password" type="password" placeholder="请输入密码" />
                </div>

                <div class="options">
                    <label class="remember">
                        <input type="checkbox" v-model="remember" />
                        记住我
                    </label>
                </div>

                <div class="error" v-if="error">{{ error }}</div>

                <button type="submit" :disabled="loading" class="btn">
                    {{ loading ? '登录中...' : '登录' }}
                </button>
            </form>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'

const username = ref('')
const password = ref('')
const remember = ref(false)
const loading = ref(false)
const error = ref('')

function validate() {
    if (!username.value.trim() || !password.value) {
        error.value = '用户名和密码不能为空'
        return false
    }
    return true
}

async function onSubmit() {
    error.value = ''
    if (!validate()) return

    loading.value = true
    try {
        // 这里替换为实际的登录请求
        await fakeLogin(username.value, password.value)
        // 登录成功后的处理，例如跳转或存 token
        // router.push('/') // 如果使用路由的话
        alert('登录成功')
    } catch (e) {
        error.value = '登录失败，用户名或密码错误'
    } finally {
        loading.value = false
    }
}

function fakeLogin(u, p) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            if (u === 'admin' && p === '123456') resolve()
            else reject(new Error('invalid'))
        }, 700)
    })
}
</script>

<style scoped>
.login-page {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #f5f7fb;
    padding: 20px;
}

.login-card {
    width: 360px;
    background: #fff;
    border-radius: 8px;
    padding: 28px;
    box-shadow: 0 6px 20px rgba(0,0,0,0.08);
}

.title {
    margin: 0 0 18px;
    text-align: center;
    font-size: 20px;
    color: #333;
}

.form .field {
    margin-bottom: 14px;
    display: flex;
    flex-direction: column;
}

.form label {
    font-size: 13px;
    color: #666;
    margin-bottom: 6px;
}

.form input[type="text"],
.form input[type="password"] {
    height: 40px;
    padding: 8px 12px;
    border: 1px solid #e6e9ef;
    border-radius: 6px;
    font-size: 14px;
    outline: none;
    transition: border-color .15s;
}

.form input:focus {
    border-color: #409eff;
}

.options {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12px;
}

.remember {
    font-size: 13px;
    color: #666;
    display: flex;
    align-items: center;
    gap: 8px;
}

.error {
    color: #e5484d;
    font-size: 13px;
    margin-bottom: 12px;
    text-align: center;
}

.btn {
    width: 100%;
    height: 42px;
    border: none;
    background: #409eff;
    color: #fff;
    border-radius: 6px;
    font-size: 15px;
    cursor: pointer;
}

.btn:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}
</style>