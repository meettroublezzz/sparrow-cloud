<script setup lang="ts">
import { computed, ref } from 'vue'
import { NButton, NCard, NInput, NTag } from 'naive-ui'

interface LibraryReference {
  title: string
  type: string
  info: string
}

interface LibraryItem {
  id: number
  name: string
  desc: string
  created: string
  accessed: string
  size: string
  color: string
  stars: number
  starred: boolean
  count: string
  owner: string
  permission: string
  references: LibraryReference[]
}

const libraryData: LibraryItem[] = [
  {
    id: 1,
    name: '工作文档库',
    desc: '团队日常办公文档与项目资料汇总',
    created: '2024-01-15',
    accessed: '2024-12-20',
    size: '2.3 GB',
    color: '#4F46E5',
    stars: 4,
    starred: true,
    count: '1,247',
    owner: '管理员',
    permission: '全员可读写',
    references: [
      { title: '项目需求文档_v3', type: 'MHTML离线网页', info: '2024-12-01 导出的完整网页归档，包含所有样式和交互' },
      { title: '设计稿参考链接', type: '网址', info: 'https://www.figma.com/file/xxx' },
      { title: '素材包下载', type: '磁力链接', info: 'magnet:?xt=urn:btih:xxxxx' },
      { title: '安装包备份', type: '种子', info: 'torrent file - 3.2GB' },
    ],
  },
  {
    id: 2,
    name: '设计资源库',
    desc: 'UI/UX 设计稿、图标、插画等素材',
    created: '2024-03-10',
    accessed: '2024-12-19',
    size: '5.7 GB',
    color: '#0891B2',
    stars: 5,
    starred: true,
    count: '852',
    owner: '设计部-张三',
    permission: '部门私有',
    references: [
      { title: '品牌设计手册', type: 'PDF文档', info: '2024年Q3更新的品牌视觉识别系统' },
      { title: 'UI组件库', type: 'Figma链接', info: '包含所有标准化原子组件' },
      { title: '高清图标包', type: 'ZIP压缩包', info: '2000+矢量图标素材' },
    ],
  },
  {
    id: 3,
    name: '摄影作品集',
    desc: '历年拍摄的摄影作品原片与后期存档',
    created: '2023-06-20',
    accessed: '2024-12-18',
    size: '12.1 GB',
    color: '#7C3AED',
    stars: 5,
    starred: false,
    count: '2,310',
    owner: '管理员',
    permission: '仅主理人',
    references: [
      { title: '西藏行摄系列', type: '相册', info: '2023年夏季西藏采风精选' },
      { title: '后期预设包', type: 'Lightroom配置', info: '个人常用胶片感滤镜预设' },
      { title: '拍摄地点清单', type: 'Excel', info: '全球摄影打卡地坐标整理' },
    ],
  },
  {
    id: 4,
    name: '代码仓库',
    desc: '项目代码、脚本工具与开源组件备份',
    created: '2024-04-05',
    accessed: '2024-12-20',
    size: '1.8 GB',
    color: '#059669',
    stars: 3,
    starred: false,
    count: '5,600',
    owner: '技术部',
    permission: '全员可读写',
    references: [
      { title: 'API接口文档', type: 'Swagger', info: '后端服务完整接口定义' },
      { title: 'CI/CD脚本', type: 'Shell', info: '自动化部署与测试流程脚本' },
      { title: '第三方库备份', type: 'GitBundle', info: '核心依赖库离线镜像' },
    ],
  },
  {
    id: 5,
    name: '学习资料库',
    desc: '课程笔记、电子书、教程与培训资源',
    created: '2023-09-12',
    accessed: '2024-12-17',
    size: '8.4 GB',
    color: '#D97706',
    stars: 4,
    starred: false,
    count: '428',
    owner: '管理员',
    permission: '公开链接',
    references: [
      { title: '全栈工程师路线', type: '思维导图', info: '从零到一的学习路径规划' },
      { title: '算法精讲视频', type: 'MP4', info: 'LeetCode热门题目深度解析' },
      { title: '前端技术周报', type: 'MHTML', info: '2024年全年前端技术趋势汇总' },
    ],
  },
  {
    id: 6,
    name: '影视资源库',
    desc: '视频素材、剪辑项目与成片归档',
    created: '2023-11-01',
    accessed: '2024-12-15',
    size: '45.6 GB',
    color: '#DC2626',
    stars: 2,
    starred: true,
    count: '92',
    owner: '视频组',
    permission: '部门私有',
    references: [
      { title: '宣传片剪辑工程', type: 'PR项目', info: '2024年度品牌宣传片初剪版' },
      { title: '4K空镜素材', type: 'MOV', info: '大疆无人机拍摄的城市空镜' },
      { title: '背景音乐库', type: 'WAV', info: '正版授权的各类风格背景音乐' },
    ],
  },
  {
    id: 7,
    name: '音乐收藏库',
    desc: '无损音频、配乐素材与播放列表整理',
    created: '2024-02-28',
    accessed: '2024-12-14',
    size: '3.2 GB',
    color: '#DB2777',
    stars: 4,
    starred: false,
    count: '875',
    owner: '管理员',
    permission: '全员只读',
    references: [
      { title: '年度听歌报告', type: 'H5网页', info: '2023年个人音乐品味总结' },
      { title: '古典音乐精选', type: 'FLAC', info: '莫扎特与贝多芬名曲无损集锦' },
      { title: '乐谱扫描件', type: 'PDF', info: '钢琴名曲纸质乐谱数字化存档' },
    ],
  },
  {
    id: 8,
    name: '个人备忘库',
    desc: '日常笔记、灵感记录与待办事项管理',
    created: '2022-12-01',
    accessed: '2024-12-20',
    size: '0.6 GB',
    color: '#2563EB',
    stars: 5,
    starred: true,
    count: '1,104',
    owner: '管理员',
    permission: '个人专用',
    references: [
      { title: '遗愿清单', type: '私密文档', info: '人生必须完成的100件事' },
      { title: '理财规划表', type: '加密Excel', info: '未来五年的资产配置与定投计划' },
      { title: '每日灵感记', type: '文本块', info: '碎片化时间的奇思妙想记录' },
    ],
  },
]

const recentIds = [1, 2, 3]
const starredIds = [2, 6, 8]

const selectedId = ref(1)
const activeTab = ref<'basic' | 'desc' | 'extra'>('basic')
const localSearch = ref('')
const librarySearch = ref('')

const selectedLibrary = computed(() => libraryData.find((item) => item.id === selectedId.value) ?? libraryData[0])

const filteredLibraries = computed(() => {
  const keyword = librarySearch.value.trim().toLowerCase()

  return libraryData.filter((item) => {
    if (keyword.length === 0) {
      return true
    }

    return [item.name, item.desc, item.owner, item.permission].join(' ').toLowerCase().includes(keyword)
  })
})

function selectLibrary(id: number): void {
  selectedId.value = id
  activeTab.value = 'basic'
}

function getPermissionClass(permission: string): string {
  const permissionMap: Record<string, string> = {
    '全员可读写': 'permission-green',
    '全员只读': 'permission-slate',
    '部门私有': 'permission-orange',
    '仅主理人': 'permission-red',
    '公开链接': 'permission-cyan',
    '个人专用': 'permission-purple',
  }

  return permissionMap[permission] ?? 'permission-blue'
}

function switchTab(tab: 'basic' | 'desc' | 'extra'): void {
  activeTab.value = tab
}

function saveDescription(): void {
  activeTab.value = 'desc'
}

const detailStars = computed(() => {
  return Array.from({ length: 5 }, (_, index) => index < selectedLibrary.value.stars)
})
</script>

<template>
  <div class="cloud-shell">
    <header class="topbar">
      <div class="brand-block">
        <div class="brand-mark">
          <span>☁</span>
        </div>
        <div class="brand-text">
          <span class="brand-title">Cloud<span>Drive</span></span>
        </div>
      </div>

      <div class="global-search">
        <span class="search-icon">⌕</span>
        <NInput v-model:value="localSearch" placeholder="搜索文件、文件库或文档..." round size="large" class="topbar-input" />
      </div>

      <div class="topbar-actions">
        <button class="icon-button" type="button">🔔</button>
        <button class="icon-button" type="button">⚙</button>
        <div class="divider"></div>
        <div class="user-chip">
          <div class="user-meta">
            <p class="user-name">管理员</p>
            <p class="user-role">超级管理员</p>
          </div>
          <div class="avatar">A</div>
        </div>
      </div>
    </header>

    <main class="workspace">
      <aside class="left-rail">
        <div class="rail-search">
          <span class="rail-search-icon">⌕</span>
          <NInput v-model:value="librarySearch" placeholder="在库中搜索..." size="small" round />
        </div>

        <section class="rail-group">
          <h3><span>◷</span>最近访问</h3>
          <div class="nav-list">
            <button
              v-for="id in recentIds"
              :key="id"
              class="nav-item"
              type="button"
              @click="selectLibrary(id)"
            >
              <span class="dot" :style="{ backgroundColor: libraryData.find((item) => item.id === id)?.color }"></span>
              <span class="nav-label">{{ libraryData.find((item) => item.id === id)?.name }}</span>
            </button>
          </div>
        </section>

        <section class="rail-group">
          <h3><span>★</span>收藏内容</h3>
          <div class="nav-list">
            <button
              v-for="id in starredIds"
              :key="id"
              class="nav-item"
              type="button"
              @click="selectLibrary(id)"
            >
              <span class="dot" :style="{ backgroundColor: libraryData.find((item) => item.id === id)?.color }"></span>
              <span class="nav-label">{{ libraryData.find((item) => item.id === id)?.name }}</span>
            </button>
          </div>
        </section>

        <section class="storage-card">
          <div class="storage-line">
            <span>已用空间</span>
            <strong>71.7 GB / 256 GB</strong>
          </div>
          <div class="progress-track"><div class="progress-fill"></div></div>
          <button class="upgrade-button" type="button">升级存储</button>
        </section>
      </aside>

      <section class="center-panel">
        <div class="section-head">
          <div class="breadcrumb">
            <span>☰</span>
            <span>所有文件库</span>
            <span>→</span>
            <strong>列表模式</strong>
          </div>
          <NButton type="primary" round class="create-button">+ 新建库</NButton>
        </div>

        <div class="library-list custom-scrollbar">
          <NCard
            v-for="item in filteredLibraries"
            :key="item.id"
            class="library-card"
            :class="{ active: selectedId === item.id }"
            @click="selectLibrary(item.id)"
          >
            <div class="library-cover" :style="{ backgroundColor: `${item.color}20` }">
              <div class="library-cover-inner" :style="{ background: `linear-gradient(135deg, ${item.color} 0%, transparent 100%)` }"></div>
              <div class="library-icon" :style="{ color: item.color }">▣</div>
            </div>

            <div class="library-body">
              <div class="library-title-row">
                <h4>{{ item.name }}</h4>
                <div class="quick-actions">
                  <button class="mini-action" type="button">✎</button>
                  <button class="mini-action danger" type="button">⌫</button>
                </div>
              </div>
              <p class="library-desc">{{ item.desc }}</p>
              <div class="library-meta">
                <span>创建: {{ item.created }}</span>
                <span>最后访问: {{ item.accessed }}</span>
                <span class="size-pill">{{ item.size }}</span>
              </div>
            </div>
          </NCard>
        </div>
      </section>

      <aside class="detail-panel">
        <div class="detail-inner">
          <div class="tab-bar">
            <button :class="['tab-btn', { active: activeTab === 'basic' }]" type="button" @mouseenter="switchTab('basic')">基本信息</button>
            <button :class="['tab-btn', { active: activeTab === 'desc' }]" type="button" @mouseenter="switchTab('desc')">文件描述</button>
            <button :class="['tab-btn', { active: activeTab === 'extra' }]" type="button" @mouseenter="switchTab('extra')">额外信息</button>
          </div>

          <div class="detail-scroll custom-scrollbar">
            <div v-if="activeTab === 'basic'" class="tab-content">
              <div class="detail-cover" :style="{ backgroundColor: selectedLibrary.color }">
                <div class="detail-cover-sheen"></div>
                <div class="library-icon white">▣</div>
              </div>
              <h2 class="detail-title">{{ selectedLibrary.name }}</h2>
              <div class="stars-row">
                <span v-for="(filled, index) in detailStars" :key="index" class="star" :class="{ filled }">★</span>
              </div>

              <NCard class="summary-card" bordered>
                <h3>库统计</h3>
                <div class="summary-grid">
                  <div><span>收藏状态</span><strong :class="selectedLibrary.starred ? 'text-blue' : 'text-muted'">{{ selectedLibrary.starred ? '已收藏' : '未收藏' }}</strong></div>
                  <div><span>创建日期</span><strong>{{ selectedLibrary.created }}</strong></div>
                  <div><span>最后修改</span><strong>{{ selectedLibrary.accessed }}</strong></div>
                  <div><span>文件数量</span><strong>{{ selectedLibrary.count }} 项</strong></div>
                </div>
              </NCard>

              <NCard class="summary-card" bordered>
                <h3>权限与所有者</h3>
                <div class="owner-block">
                  <div class="block-row">
                    <span class="muted-label">所有者</span>
                    <span class="owner-pill">
                      <span class="owner-avatar">{{ selectedLibrary.owner.charAt(0) }}</span>
                      {{ selectedLibrary.owner }}
                    </span>
                  </div>
                  <div class="block-row">
                    <span class="muted-label">访问权限</span>
                    <NTag :class="getPermissionClass(selectedLibrary.permission)" :bordered="false" round>
                      🔒 {{ selectedLibrary.permission }}
                    </NTag>
                  </div>
                </div>
              </NCard>
            </div>

            <div v-else-if="activeTab === 'desc'" class="tab-content">
              <h3 class="section-title">库描述信息</h3>
              <textarea class="desc-editor" :value="selectedLibrary.desc" placeholder="请输入库描述..."></textarea>
              <NButton type="primary" round block class="save-button" @click="saveDescription">保存修改</NButton>
            </div>

            <div v-else class="tab-content">
              <h3 class="section-title">引用信息列表</h3>
              <div class="ref-list">
                <div v-for="ref in selectedLibrary.references" :key="ref.title" class="ref-item">
                  <div class="ref-main">
                    <strong>{{ ref.title }}</strong>
                    <span>{{ ref.type }}</span>
                  </div>
                  <div class="ref-arrow">→</div>
                </div>
              </div>

              <div class="hint-box">
                <span>ℹ</span>
                <p>悬停在条目上可查看详细说明</p>
              </div>
            </div>
          </div>

          <div class="footer-actions">
            <NButton secondary round class="footer-btn">共享库</NButton>
            <NButton round class="footer-btn dark">导出</NButton>
          </div>
        </div>
      </aside>
    </main>
  </div>
</template>

<style scoped>
.cloud-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  color: #1e293b;
  background: #f8fafc;
}

.topbar {
  height: 64px;
  flex: none;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  padding: 0 24px;
  background: rgba(255, 255, 255, 0.96);
  border-bottom: 1px solid #e2e8f0;
  box-shadow: 0 1px 0 rgba(15, 23, 42, 0.02);
  backdrop-filter: blur(16px);
}

.brand-block,
.user-chip,
.topbar-actions,
.library-title-row,
.library-meta,
.breadcrumb,
.storage-line,
.block-row,
.tab-bar,
.footer-actions,
.topbar-actions {
  display: flex;
  align-items: center;
}

.brand-block {
  width: 256px;
  gap: 10px;
}

.brand-mark {
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  color: white;
  background: linear-gradient(135deg, #2563eb 0%, #3b82f6 100%);
  box-shadow: 0 8px 18px rgba(37, 99, 235, 0.25);
}

.brand-title {
  font-size: 20px;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.brand-title span {
  color: #2563eb;
}

.global-search {
  flex: 1;
  max-width: 700px;
  position: relative;
}

.search-icon,
.rail-search-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
}

.topbar-input {
  width: 100%;
}

.topbar-actions {
  gap: 12px;
}

.icon-button {
  width: 36px;
  height: 36px;
  border: 0;
  border-radius: 999px;
  background: #f1f5f9;
}

.divider {
  width: 1px;
  height: 28px;
  background: #e2e8f0;
}

.user-chip {
  gap: 12px;
}

.user-name,
.user-role {
  margin: 0;
}

.workspace {
  flex: 1;
  display: grid;
  grid-template-columns: 300px 1fr 360px;
  gap: 20px;
  padding: 20px;
}

.left-rail,
.center-panel,
.detail-panel {
  min-height: 0;
}
</style>