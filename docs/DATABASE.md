# 虹乡胡氏安定堂家谱管理系统 - 数据字典

## 数据库：SQLite (AES 加密)

### 核心表结构

---

#### 1. persons（人物主表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 人物唯一ID |
| name | TEXT | NOT NULL | 姓名 |
| original_name | TEXT | | 原名/字/号 |
| gender | INTEGER | NOT NULL | 0=女 1=男 |
| birth_date | TEXT | | 出生日期（公历存储） |
| death_date | TEXT | | 逝世日期 |
| burial_place | TEXT | | 墓葬地点 |
| spouse_ids | TEXT | | 配偶ID列表(JSON数组) |
| father_id | INTEGER | FK→persons.id | 父亲ID |
| mother_id | INTEGER | FK→persons.id | 母亲ID |
| birth_order | INTEGER | | 排行 |
| education | TEXT | | 学历 |
| occupation | TEXT | | 职业 |
| honors | TEXT | | 功名/荣誉 |
| residence | TEXT | | 居住地 |
| contact_phone | TEXT | | 联系电话（脱敏存储） |
| contact_wechat | TEXT | | 微信号 |
| photo_path | TEXT | | 个人照片路径 |
| scan_paths | TEXT | | 扫描件路径(JSON数组) |
| audio_paths | TEXT | | 口述史录音路径(JSON数组) |
| relationship_tags | TEXT | | 关系标签(JSON: 过继/入赘/兼祧/收养) |
| lunar_birth | TEXT | | 农历出生日期（展示层） |
| gan_zhi_birth | TEXT | | 干支出生日期 |
| age_at_death | INTEGER | | 享年（自动计算） |
| status | INTEGER | DEFAULT 0 | 0=正常 1=已故 2=待审核 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | 创建时间 |
| updated_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | 更新时间 |
| version | INTEGER | DEFAULT 1 | 乐观锁版本号 |

---

#### 2. families（家族/房头表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 家族ID |
| name | TEXT | NOT NULL | 家族/房头名称 |
| ancestor_id | INTEGER | FK→persons.id | 始祖ID |
| generation_count | INTEGER | | 代数 |
| description | TEXT | | 家族简介 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |
| updated_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |

---

#### 3. generations（世代表 - 用于世系排序）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 世代ID |
| family_id | INTEGER | FK→families.id | 所属家族 |
| generation_number | INTEGER | NOT NULL | 世代编号（1,2,3...） |
| generation_name | TEXT | | 世代命名（如"第X世"） |
| character_pattern | TEXT | | 字辈谱 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |

---

#### 4. pending_submissions（待审提交表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 提交ID |
| submitter_id | INTEGER | FK→users.id | 提交者ID |
| person_data | TEXT | NOT NULL | 提交的人物数据(JSON) |
| family_id | INTEGER | FK→families.id | 关联家族 |
| submission_type | INTEGER | NOT NULL | 1=新增 2=修改 3=素材上传 |
| status | INTEGER | DEFAULT 0 | 0=待审核 1=已采纳 2=已驳回 3=需补充 |
| supplement_reason | TEXT | | 需补充的原因 |
| reviewer_id | INTEGER | FK→users.id | 审核者ID |
| review_comment | TEXT | | 审核备注 |
| submitted_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | 提交时间 |
| reviewed_at | DATETIME | | 审核时间 |
| synced_to_main | BOOLEAN | DEFAULT 0 | 是否已同步到主谱 |

---

#### 5. materials（素材表 - 照片/录音/扫描件）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 素材ID |
| person_id | INTEGER | FK→persons.id | 关联人物 |
| material_type | INTEGER | NOT NULL | 1=墓碑照片 2=老照片 3=口述录音 4=扫描件 |
| file_path | TEXT | NOT NULL | 本地文件路径 |
| cloud_hash | TEXT | | 云端哈希（去重用） |
| upload_status | INTEGER | DEFAULT 0 | 0=未上传 1=已上传 |
| ocr_result | TEXT | | OCR识别结果 |
| ocr_confirmed | BOOLEAN | DEFAULT 0 | 是否人工确认 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |

---

#### 6. users（用户表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 用户ID |
| username | TEXT | NOT NULL UNIQUE | 用户名 |
| password_hash | TEXT | NOT NULL | 密码哈希 |
| display_name | TEXT | NOT NULL | 显示名称 |
| role | INTEGER | NOT NULL | 0=总管理员 1=房头管理员 2=编辑员 3=只读用户 |
| family_id | INTEGER | FK→families.id | 所属家族 |
| phone | TEXT | | 手机号（脱敏） |
| wechat_id | TEXT | | 微信号 |
| device_id | TEXT | | 设备ID（APP端） |
| is_active | BOOLEAN | DEFAULT 1 | 是否启用 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |
| last_login_at | DATETIME | | 最后登录时间 |

---

#### 7. audit_log（操作审计日志表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 日志ID |
| user_id | INTEGER | FK→users.id | 操作用户 |
| action | TEXT | NOT NULL | 操作类型 |
| target_type | TEXT | | 目标类型(person/family/material) |
| target_id | INTEGER | | 目标ID |
| old_value | TEXT | | 变更前值(JSON) |
| new_value | TEXT | | 变更后值(JSON) |
| ip_address | TEXT | | IP地址 |
| device_info | TEXT | | 设备信息 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |

---

#### 8. genealogy_tree（世系树缓存表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 记录ID |
| person_id | INTEGER | FK→persons.id | 人物ID |
| tree_level | INTEGER | | 树层级 |
| parent_id | INTEGER | FK→persons.id | 父级ID |
| left_index | INTEGER | | 左索引（嵌套集算法） |
| right_index | INTEGER | | 右索引 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |

---

#### 9. system_config（系统配置表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| key | TEXT | PK | 配置键 |
| value | TEXT | NOT NULL | 配置值 |
| description | TEXT | | 配置说明 |
| updated_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |

**预置配置：**
- `sealed` = `false`（是否已封谱）
- `reset_code` = `""`（解封码，加密存储）
- `generation_character` = `""`（字辈谱）
- `family_description` = `""`（家族简介）
- `backup_enabled` = `true`（自动备份开关）
- `backup_interval_hours` = `24`（备份间隔）
- `ocr_enabled` = `true`（OCR开关）
- `theme_mode` = `light`（主题：light/dark）
- `date_format` = `gregorian`（历法展示：gregorian/lunar/ganzhi）

---

#### 10. sync_queue（同步队列表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | INTEGER | PK AUTOINCREMENT | 队列ID |
| direction | INTEGER | NOT NULL | 0=上行(APP→云) 1=下行(云→EXE) |
| entity_type | TEXT | NOT NULL | 实体类型 |
| entity_id | INTEGER | | 实体ID |
| payload | TEXT | NOT NULL | 数据载荷(JSON) |
| status | INTEGER | DEFAULT 0 | 0=待同步 1=已同步 2=失败 |
| retry_count | INTEGER | DEFAULT 0 | 重试次数 |
| created_at | DATETIME | DEFAULT CURRENT_TIMESTAMP | |
| synced_at | DATETIME | | 同步时间 |

---

### 索引

```sql
-- 人物查询优化
CREATE INDEX idx_persons_father ON persons(father_id);
CREATE INDEX idx_persons_family ON persons(family_id);
CREATE INDEX idx_persons_generation ON persons(generation_number);
CREATE INDEX idx_persons_name ON persons(name);
CREATE INDEX idx_persons_status ON persons(status);

-- 待审提交
CREATE INDEX idx_pending_submitter ON pending_submissions(submitter_id);
CREATE INDEX idx_pending_status ON pending_submissions(status);

-- 素材
CREATE INDEX idx_materials_person ON materials(person_id);
CREATE INDEX idx_materials_type ON materials(material_type);

-- 审计日志
CREATE INDEX idx_audit_user ON audit_log(user_id);
CREATE INDEX idx_audit_time ON audit_log(created_at);

-- 同步队列
CREATE INDEX idx_sync_direction ON sync_queue(direction, status);
```

---

### 数据加密策略

- SQLite 使用 SQLCipher AES-256 加密
- 密钥从 Windows DPAPI / Android Keystore 获取
- 备份文件额外使用 RSA 加密
- 敏感字段（phone/wechat）应用 AES 加密后存储
