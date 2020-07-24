package com.hideakin.textsearch.index.service;

import java.time.ZonedDateTime;
import java.util.Collection;
import java.util.List;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.hideakin.textsearch.index.entity.FileContentEntity;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.ObjectDisposition;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;
import com.hideakin.textsearch.index.repository.FileContentRepository;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.utility.GZipHelper;

@Service
@Transactional
public class FileServiceImpl implements FileService {

	private static final Logger logger = LoggerFactory.getLogger(FileServiceImpl.class);

	@Autowired
	private FileRepository fileRepository;
	
	@Autowired
	private FileGroupRepository fileGroupRepository;
	
	@Autowired
	private FileContentRepository fileContentRepository;
	
	@Autowired
	private PreferenceRepository preferenceRepository;

	@Override
	public FileInfo[] getFiles(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGidAndStaleFalse(fileGroupEntity.getGid());
		int count = entities.size();
		if (count == 0) {
			return new FileInfo[0];
		}
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			values[index] = new FileInfo(entities.get(index), fileGroupEntity);
		}
		return values;
	}

	@Override
	public FileStats getStats(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		FileStats stats = new FileStats(fileGroupEntity.getGid(), fileGroupEntity.getName());
		List<FileEntity> entities = fileRepository.findAllByGid(fileGroupEntity.getGid());
		for (FileEntity e : entities) {
			FileContentEntity fc = fileContentRepository.findByFid(e.getFid());
			if (e.isStale()) {
				stats.incStaleFiles();
				stats.addStaleBytes(e.getSize());
				stats.addStoredStaleBytes(fc.getData().length);
			} else {
				stats.incFiles();
				stats.addBytes(e.getSize());
				stats.addStoredBytes(fc.getData().length);
			}
		}
		return stats;
	}
	
	@Override
	public FileInfo getFile(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity != null && !entity.isStale()) {
			FileGroupEntity fileGroupEntity = fileGroupRepository.findByGid(entity.getGid());
			return new FileInfo(entity, fileGroupEntity);
		} else {
			return null;
		}
	}
	
	@Override
	public FileInfo getFile(String group, String path) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGidAndPathAndStaleFalse(fileGroupEntity.getGid(), path);
		if (entities.size() == 0) {
			return null;
		}
		FileEntity entity = null;
		for (FileEntity next : entities) {
			if (entity == null) {
				entity = next;
			} else if (next.getUpdatedAt().compareTo(entity.getUpdatedAt()) > 0) {
				// unexpected case
				entity.setStale(true);
				fileRepository.save(entity);
				entity = next;
			} else {
				// unexpected case, too
				next.setStale(true);
				fileRepository.save(next);
			}
		}
		return entity != null && !entity.isStale() ? new FileInfo(entity, fileGroupEntity) : null;
	}

	@Override
	public String getPath(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		return entity != null && !entity.isStale() ? entity.getPath() : null;
	}

	@Override
	public byte[] getContents(int fid) {
		FileEntity fileEntity = fileRepository.findByFid(fid);
		if (fileEntity == null || fileEntity.isStale()) {
			return null;
		}
		FileContentEntity fileContentEntity = fileContentRepository.findByFid(fid);
		return fileContentEntity != null ? GZipHelper.decompress(fileContentEntity.getData(), fileEntity.getSize()) : null;
	}
	
	@Override
	public FileInfo addFile(String group, String path, int length, ObjectDisposition disp) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByNameForUpdate(group);
		if (fileGroupEntity == null) {
			disp.setValue(ObjectDisposition.GROUP_NOT_FOUND);
			return null;
		}
		final int gid = fileGroupEntity.getGid();
		List<FileEntity> entities = fileRepository.findAllByGidAndPathAndStaleFalse(gid, path);
		for (FileEntity e : entities) {
			e.setStale(true);
			fileRepository.save(e);
		}
		if (entities.size() == 0) {
			disp.setValue(ObjectDisposition.CREATED);
		} else {
			disp.setValue(ObjectDisposition.UPDATED);
		}
		FileEntity entity = fileRepository.save(new FileEntity(getNextFid(), path, length, gid));
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return new FileInfo(entity, fileGroupEntity);
	}

	@Override
	public FileInfo updateFile(int fid, String path, int length) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity == null) {
			return null;
		}
		final int gid = entity.getGid();
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByGidForUpdate(gid);
		if (fileGroupEntity == null) {
			logger.error("Existing File entity {} has an invalid GID {}.", fid, gid);
			return null;
		}
		entity.setStale(true);
		fileRepository.save(entity);
		entity = fileRepository.save(new FileEntity(getNextFid(), path, length, gid));
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return new FileInfo(entity, fileGroupEntity);
	}
	
	@Override
	public FileInfo[] deleteFiles(int gid) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByGidForUpdate(gid);
		if (fileGroupEntity == null) {
			return new FileInfo[0];
		}
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		int count = entities.size();
		if (count == 0) {
			return new FileInfo[0];
		}
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			FileEntity entity = entities.get(index);
			values[index] = new FileInfo(entity, fileGroupEntity);
		}
		fileRepository.deleteByGid(gid);
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return values;
	}

	@Override
	public FileInfo[] deleteStaleFiles(int gid) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByGidForUpdate(gid);
		if (fileGroupEntity == null) {
			return new FileInfo[0];
		}
		List<FileEntity> entities = fileRepository.findAllByGidAndStaleTrue(gid);
		int count = entities.size();
		if (count == 0) {
			return new FileInfo[0];
		}
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			FileEntity entity = entities.get(index);
			values[index] = new FileInfo(entity, fileGroupEntity);
			fileRepository.deleteByFid(entity.getFid());
		}
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return values;
	}

	@Override
	public FileInfo deleteFile(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity == null) {
			return null;
		}
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByGidForUpdate(entity.getGid());
		if (fileGroupEntity == null) {
			return null;
		}
		fileRepository.deleteByFid(fid);
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return new FileInfo(entity, fileGroupEntity);
	}

	@Override
	public void addContents(int fid, byte[] data) {
		fileContentRepository.save(new FileContentEntity(fid, data));
	}

	@Override
	public void deleteContents(int fid) {
		fileContentRepository.deleteByFid(fid);
	}

	@Override
	public void deleteContents(Collection<Integer> fids) {
		fileContentRepository.deleteByFidIn(fids);
	}

	private int getNextFid() {
		int nextId;
		final String name = "FID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			nextId = entity.getIntValue();
		} else {
			entity = new PreferenceEntity(name);
			Integer maxId = fileRepository.getMaxFid();
			nextId = (maxId != null ? maxId : 0) + 1;
		}
		entity.setValue(nextId + 1);
		preferenceRepository.save(entity);
		return nextId;
	}
	
}
