package com.hideakin.textsearch.index.service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.Distribution;
import com.hideakin.textsearch.index.model.PathPositions;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.TextRepository;
import com.hideakin.textsearch.index.utility.DistributionDecoder;

@Service
@Transactional
public class IndexServiceImpl implements IndexService {

	@Autowired
	private FileGroupRepository fileGroupRepository;

	@Autowired
	private FileRepository fileRepository;

	@Autowired
	private TextRepository textRepository;

	@Override
	public PathPositions[] findText(String group, String text, SearchOptions option) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return new PathPositions[0];
		}
		int gid = fileGroupEntity.getGid();
		if (option == SearchOptions.Exact) {
			TextEntity textEntity = textRepository.findByText(text);
			if (textEntity != null) {
				Map<Integer,PathPositions> map = new HashMap<Integer,PathPositions>();
				populateHitMap(map, textEntity, gid);
				return map.values().toArray(new PathPositions[map.size()]);
			}
		} else {
			List<TextEntity> textEntities;
			if (option == SearchOptions.Contains) {
				textEntities = textRepository.findAllByTextContaining(text);
			} else if (option == SearchOptions.StartsWith) {
				textEntities = textRepository.findAllByTextStartingWith(text);
			} else if (option == SearchOptions.EndsWith) {
				textEntities = textRepository.findAllByTextEndingWith(text);
			} else {
				textEntities = null;
			}
			if (textEntities != null) {
				Map<Integer,PathPositions> map = new HashMap<Integer,PathPositions>();
				populateHitMap(map, textEntities, gid);
				return map.values().toArray(new PathPositions[map.size()]);
			}
		}
		return new PathPositions[0];
	}


	private void populateHitMap(Map<Integer,PathPositions> map, List<TextEntity> entities, int gid) {
		for (TextEntity entity : entities) {
			populateHitMap(map, entity, gid);
		}
	}

	private void populateHitMap(Map<Integer,PathPositions> map, TextEntity textEntity, int gid) {
		DistributionDecoder dec = new DistributionDecoder(textEntity.getDist());
		for (Distribution dist = dec.get(); dist != null; dist = dec.get()) {
			int fid = dist.getFid();
			PathPositions pp = map.get(fid);
			if (pp != null) {
				pp.addPositions(dist.getPositions());
			} else {
				FileEntity fileEntity = fileRepository.findByFid(fid);
				String path = fileEntity != null ? fileEntity.getPath() : null;
				if (path != null) {
					pp = new PathPositions();
					pp.setPath(path);
					pp.setPositions(dist.getPositions());
					map.put(fid, pp);
				}
			}
		}
	}

}
