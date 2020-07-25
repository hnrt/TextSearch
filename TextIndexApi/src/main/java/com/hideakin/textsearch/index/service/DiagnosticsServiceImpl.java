package com.hideakin.textsearch.index.service;

import java.util.ArrayList;
import java.util.List;

import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.IdStatus;
import com.hideakin.textsearch.index.model.IndexStats;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.FileContentRepository;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.repository.TextExRepository;
import com.hideakin.textsearch.index.repository.UserRepository;

@Service
@Transactional
public class DiagnosticsServiceImpl implements DiagnosticsService {

	@Autowired
	private UserRepository userRepository;
	
	@Autowired
	private FileGroupRepository groupRepository;
	
	@Autowired
	private FileRepository fileRepository;
	
	@Autowired
	private FileContentRepository contentRepository;

	@Autowired
	private TextExRepository textExRepository;

	@Autowired
	private PreferenceRepository prefRepository;

	@Override
	public IdStatus getIds() {
		PreferenceEntity uidNext = prefRepository.findByName("UID.next");
		PreferenceEntity gidNext = prefRepository.findByName("GID.next");
		PreferenceEntity fidNext = prefRepository.findByName("FID.next");
		return new IdStatus(
				uidNext != null ? uidNext.getIntValue() : -1,
				gidNext != null ? gidNext.getIntValue() : -1,
				fidNext != null ? fidNext.getIntValue() : -1);
	}

	@Override
	public IdStatus resetIds() {
		Integer maxUid = userRepository.getMaxUid();
		if (maxUid == null) {
			maxUid = 0;
		}
		Integer maxGid = groupRepository.getMaxGid();
		if (maxGid == null) {
			maxGid = 0;
		}
		Integer maxFid = fileRepository.getMaxFid();
		if (maxFid == null) {
			maxFid = 0;
		}
		return new IdStatus(
				prefRepository.save(new PreferenceEntity("UID.next", maxUid + 1)).getIntValue(),
				prefRepository.save(new PreferenceEntity("GID.next", maxGid + 1)).getIntValue(),
				prefRepository.save(new PreferenceEntity("FID.next", maxFid + 1)).getIntValue());
	}

	@Override
	public FileInfo[] findUnusedFiles() {
		List<FileEntity> entities = fileRepository.findUnused();
		FileInfo[] array = new FileInfo[entities.size()];
		int index = 0;
		for (FileEntity entity : entities) {
			array[index++] = new FileInfo(entity.getFid(), entity.getPath(), entity.getSize(), -1, null);
		}
		return array;
	}

	@Override
	public FileInfo[] deleteUnusedFiles() {
		List<FileEntity> entities = fileRepository.findUnused();
		FileInfo[] array = new FileInfo[entities.size()];
		int index = 0;
		List<Integer> fids = new ArrayList<>(entities.size());
		for (FileEntity entity : entities) {
			array[index++] = new FileInfo(entity.getFid(), entity.getPath(), entity.getSize(), -1, null);
			fids.add(entity.getFid());
		}
		fileRepository.deleteByFidIn(fids);
		return array;
	}

	@Override
	public int[] findUnusedContents() {
		List<Integer> fids = contentRepository.findUnusedFids();
		int[] array = new int[fids.size()];
		int index = 0;
		for (Integer fid : fids) {
			array[index++] = fid;
		}
		return array;
	}

	@Override
	public int[] deleteUnusedContents() {
		List<Integer> fids = contentRepository.findUnusedFids();
		int[] array = new int[fids.size()];
		int index = 0;
		for (Integer fid : fids) {
			array[index++] = fid;
		}
		contentRepository.deleteByFidIn(fids);
		return array;
	}

	@Override
	public IndexStats getIndexStats(String group) {
		FileGroupEntity groupEntity = groupRepository.findByName(group);
		if (groupEntity == null) {
			return null;
		}
		final int gid = groupEntity.getGid();
		IndexStats s = new IndexStats();
		s.setGid(gid);
		s.setGroup(group);
		{
			List<FileEntity> fileEntities = fileRepository.findAllByGidAndStaleFalse(gid);
			s.setFileCount(fileEntities.size());
		}
		int textCount = 0;
		long totalBytes = 0;
		long totalCount = 0;
		final int limit = 4096;
		for (int offset = 0; true; offset += limit) {
			List<TextEntity> textEntities = textExRepository.findByGid(gid, limit, offset);
			if (textEntities.size() == 0) {
				break;
			}
			textCount += textEntities.size();
			for (TextEntity textEntity : textEntities) {
				byte[] data = textEntity.getDist();
				if (data == null) {
					textCount--;
					continue;
				}
				totalBytes += data.length;
				TextDistribution.PackedSequence sequence = TextDistribution.sequence(data);
				for (TextDistribution td = sequence.get(); td != null; td = sequence.get()) {
					totalCount += 1 + 1 + td.getPositions().length;
				}
			}
		}
		s.setTextCount(textCount);
		s.setTotalBytes(totalBytes);
		s.setTotalCount(totalCount);
		return s;
	}

}
