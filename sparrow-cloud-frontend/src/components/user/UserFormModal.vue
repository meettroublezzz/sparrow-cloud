<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { NButton, NForm, NFormItem, NModal, NRadio, NRadioGroup, NSpace, NInput, NSelect } from 'naive-ui'
import type { FormInst, FormRules } from 'naive-ui'
import type { UserFormState } from '../../types/user'

const props = defineProps<{
  show: boolean
  loading: boolean
  modelValue: UserFormState
  title: string
}>()

const emit = defineEmits<{
  'update:show': [value: boolean]
  'update:modelValue': [value: UserFormState]
  submit: []
}>()

const formRef = ref<FormInst | null>(null)

const rules: FormRules = {
  userName: [
    { required: true, message: '请输入用户名', trigger: ['input', 'blur'] },
    { min: 3, max: 32, message: '用户名长度应在 3 到 32 个字符之间', trigger: ['input', 'blur'] },
  ],
  displayName: [{ required: true, message: '请输入昵称', trigger: ['input', 'blur'] }],
  email: [
    { required: true, message: '请输入邮箱', trigger: ['input', 'blur'] },
    { type: 'email', message: '请输入有效邮箱地址', trigger: ['input', 'blur'] },
  ],
}

const formModel = reactive<UserFormState>({
  userName: '',
  displayName: '',
  email: '',
  role: 'User',
  status: 'active',
})

watch(
  () => props.modelValue,
  (value) => {
    formModel.userName = value.userName
    formModel.displayName = value.displayName
    formModel.email = value.email
    formModel.role = value.role
    formModel.status = value.status
  },
  { immediate: true, deep: true },
)

const visible = computed({
  get: () => props.show,
  set: (value: boolean) => emit('update:show', value),
})

function handleClose(): void {
  visible.value = false
}

async function handleSubmit(): Promise<void> {
  await formRef.value?.validate()
  emit('update:modelValue', { ...formModel })
  emit('submit')
}
</script>

<template>
  <n-modal v-model:show="visible" preset="card" class="user-form-modal" :title="title" :mask-closable="false">
    <n-form ref="formRef" :model="formModel" :rules="rules" label-placement="left" label-width="90">
      <n-form-item label="用户名" path="userName">
        <n-input v-model:value="formModel.userName" placeholder="请输入用户名" />
      </n-form-item>
      <n-form-item label="昵称" path="displayName">
        <n-input v-model:value="formModel.displayName" placeholder="请输入昵称" />
      </n-form-item>
      <n-form-item label="邮箱" path="email">
        <n-input v-model:value="formModel.email" placeholder="请输入邮箱" />
      </n-form-item>
      <n-form-item label="角色">
        <n-select
          v-model:value="formModel.role"
          :options="[
            { label: '管理员', value: 'Admin' },
            { label: '普通用户', value: 'User' },
          ]"
        />
      </n-form-item>
      <n-form-item label="状态">
        <n-radio-group v-model:value="formModel.status">
          <n-space>
            <n-radio value="active">启用</n-radio>
            <n-radio value="inactive">停用</n-radio>
          </n-space>
        </n-radio-group>
      </n-form-item>
    </n-form>

    <template #footer>
      <n-space justify="end">
        <n-button @click="handleClose">取消</n-button>
        <n-button type="primary" :loading="loading" @click="handleSubmit">保存</n-button>
      </n-space>
    </template>
  </n-modal>
</template>

<style scoped>
.user-form-modal {
  width: min(560px, calc(100vw - 32px));
}
</style>
