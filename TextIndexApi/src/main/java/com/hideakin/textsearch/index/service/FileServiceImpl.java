package com.hideakin.textsearch.index.service;

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.hideakin.textsearch.index.entity.FileContentEntity;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.Distribution;
import com.hideakin.textsearch.index.model.FileDisposition;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.repository.FileContentRepository;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.repository.TextRepository;
import com.hideakin.textsearch.index.utility.ContentType;
import com.hideakin.textsearch.index.utility.DistributionDecoder;
import com.hideakin.textsearch.index.utility.GZipHelper;
import com.hideakin.textsearch.index.utility.TextEncoding;
import com.hideakin.textsearch.index.utility.TextTokenizer;

@Service
@Transactional
public class FileServiceImpl implements FileService {

	private static final Logger logger = LoggerFactory.getLogger(FileServiceImpl.class);

	private static final String UTF_8 = "UTF-8";

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileRepository fileRepository;
	
	@Autowired
	private FileGroupRepository fileGroupRepository;
	
	@Autowired
	private FileContentRepository fileContentRepository;
	
	@Autowired
	private PreferenceRepository preferenceRepository;

	@Autowired
	private TextRepository textRepository;

	@Override
	public FileInfo[] getFiles(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGid(fileGroupEntity.getGid());
		int count;
		if (entities == null || (count = entities.size()) == 0) {
			return new FileInfo[0];
		}
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			values[index] = new FileInfo(entities.get(index), fileGroupEntity);
		}
		return values;
	}
	
	@Override
	public FileInfo getFile(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity != null) {
			FileGroupEntity fileGroupEntity = fileGroupRepository.findByGid(entity.getGid());
			return new FileInfo(entity, fileGroupEntity);
		} else {
			return null;
		}
	}

	@Override
	public String getPath(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		return entity != null ? entity.getPath() : null;
	}
	
	@Override
	public FileInfo addFile(String group, String path, byte[] data, String contentType, FileDisposition disp) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			disp.setValue(FileDisposition.GROUP_NOT_FOUND);
			return null;
		}
		int gid = fileGroupEntity.getGid();
		int changes = 0;
		List<FileEntity> entities = fileRepository.findAllByGidAndPath(gid, path);
		if (entities != null) {
			for (FileEntity e : entities) {
				if (!e.isStale()) {
					e.setStale(true);
					fileRepository.save(e);
					changes++;
				}
			}
		}
		if (changes == 0) {
			disp.setValue(FileDisposition.CREATED);
		} else {
			disp.setValue(FileDisposition.UPDATED);
		}
		FileEntity entity = saveFile(gid, path, data, contentType);
		return new FileInfo(entity, fileGroupEntity);
	}

	@Override
	public FileInfo updateFile(int fid, String path, byte[] data, String contentType) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity == null) {
			return null;
		}
		entity.setStale(true);
		fileRepository.save(entity);
		int gid = entity.getGid();
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByGid(gid);
		if (fileGroupEntity == null) {
			logger.error("Existing File entity {} has an invalid GID {}.", fid, gid);
			return null;
		}
		entity = saveFile(gid, path, data, contentType);
		return new FileInfo(entity, fileGroupEntity);
	}
	
	@Override
	public FileInfo[] deleteFiles(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		int gid = fileGroupEntity.getGid();
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		int count;
		if (entities == null || (count = entities.size()) == 0) {
			return new FileInfo[0];
		}
		Set<Integer> fids = new HashSet<>(count);
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			FileEntity entity = entities.get(index);
			int fid = entity.getFid();
			fids.add(fid);
			values[index] = new FileInfo(entity, fileGroupEntity);
			fileContentRepository.deleteByFid(fid);
		}
		fileRepository.deleteByGid(gid);
		removeDistribution(fids);
		return values;
	}

	@Override
	public FileInfo deleteFile(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity != null) {
			FileGroupEntity fileGroupEntity = fileGroupRepository.findByGid(entity.getGid());
			fileContentRepository.deleteByFid(fid);
			fileRepository.deleteByFid(fid);
			removeDistribution(fid);
			return new FileInfo(entity, fileGroupEntity);
		} else {
			return null;
		}
	}

	@Override
	public byte[] getFileContents(int fid) {
		FileEntity fileEntity = fileRepository.findByFid(fid);
		if (fileEntity == null) {
			return null;
		}
		FileContentEntity fileContentEntity = fileContentRepository.findByFid(fid);
		return fileContentEntity != null ? GZipHelper.decompress(fileContentEntity.getData(), fileEntity.getSize()) : null;
	}

	private int getNextFid() {
		int nextId;
		final String name = "FID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			nextId = entity.getIntValue();
		} else {
			entity = new PreferenceEntity(name);
			Integer maxId = (Integer)em.createQuery("SELECT MAX(fid) FROM files").getSingleResult();
			nextId = (maxId != null ? maxId : 0) + 1;
		}
		entity.setValue(nextId + 1);
		preferenceRepository.save(entity);
		return nextId;
	}

	private FileEntity saveFile(int gid, String path, byte[] data, String contentType) {
		data = convertToUTF8(data, contentType);
		FileEntity entity = fileRepository.save(new FileEntity(getNextFid(), path, data.length, gid));
		int fid = entity.getFid();
		FileContentEntity fcEntity = new FileContentEntity(fid, GZipHelper.compress(data));
        fileContentRepository.save(fcEntity);
		TextTokenizer tokenizer = new TextTokenizer();
		tokenizer.run(data, UTF_8);
		applyTextMap(fid, tokenizer.populateTextMap());
		return entity;
	}

	private void removeDistribution(int fid) {
		Set<Integer> fids = new HashSet<>(1);
		fids.add(fid);
		removeDistribution(fids);
	}

	private void removeDistribution(Set<Integer> fids) {
		List<String> texts = getAllTexts();
		for (String text : texts) {
			TextEntity textEntity = textRepository.findByText(text);
			DistributionDecoder dec = new DistributionDecoder(textEntity.getDist());
			textEntity.setDist(null);
			for (Distribution dist = dec.get(); dist != null; dist = dec.get()) {
				if (!fids.contains(dist.getFid())) {
					textEntity.appendDist(dist);
				}
			}
			if (textEntity.getDist() != null && textEntity.getDist().length > 0) {
				textRepository.save(textEntity);
			} else {
				textRepository.delete(textEntity);
			}
		}
	}

	@SuppressWarnings("unchecked")
	private List<String> getAllTexts() {
		return (List<String>)em.createQuery("SELECT text FROM texts").getResultList();
	}

	private void applyTextMap(int fid, Map<String,List<Integer>> map) {
		for (String key : map.keySet()) {
			List<Integer> positions = map.get(key);
			TextEntity entity = textRepository.findByText(key);
			if (entity == null) {
				entity = new TextEntity();
				entity.setText(key);
			}
			entity.appendDist(fid, positions);
			textRepository.save(entity);
		}
	}
	
	private static byte[] convertToUTF8(byte[] data, String contentType) {
		ContentType ct = ContentType.parse(contentType);
		Charset cs = ct.getCharset(UTF_8);
		if (!cs.displayName().equals(UTF_8)) {
			data = TextEncoding.convert(data, cs, StandardCharsets.UTF_8);
		}
		return data;
	}

}
