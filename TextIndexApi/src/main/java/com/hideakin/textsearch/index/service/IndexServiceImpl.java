package com.hideakin.textsearch.index.service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.TextRepository;

@Service
@Transactional
public class IndexServiceImpl implements IndexService {

	@Autowired
	private FileGroupRepository fileGroupRepository;

	@Autowired
	private TextRepository textRepository;

	@Override
	public TextDistribution[] findText(String group, String text, SearchOptions option) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		int gid = fileGroupEntity.getGid();
		Map<Integer,TextDistribution> map = new HashMap<Integer,TextDistribution>();
		if (option == SearchOptions.Exact) {
			TextEntity textEntity = textRepository.findByTextAndGid(text, gid);
			if (textEntity == null) {
				return new TextDistribution[0];
			}
			populateHitMap(map, textEntity);
		} else {
			List<TextEntity> textEntities;
			if (option == SearchOptions.Contains) {
				textEntities = textRepository.findAllByTextContainingAndGid(text, gid);
			} else if (option == SearchOptions.StartsWith) {
				textEntities = textRepository.findAllByTextStartingWithAndGid(text, gid);
			} else if (option == SearchOptions.EndsWith) {
				textEntities = textRepository.findAllByTextEndingWithAndGid(text, gid);
			} else {
				return new TextDistribution[0]; // never reach here
			}
			populateHitMap(map, textEntities);
		}
		return map.values().toArray(new TextDistribution[map.size()]);
	}

	private void populateHitMap(Map<Integer,TextDistribution> map, List<TextEntity> entities) {
		for (TextEntity entity : entities) {
			populateHitMap(map, entity);
		}
	}

	private void populateHitMap(Map<Integer,TextDistribution> map, TextEntity textEntity) {
		TextDistribution.PackedSequence sequence = TextDistribution.sequence(textEntity.getDist());
		for (TextDistribution dist = sequence.get(); dist != null; dist = sequence.get()) {
			int fid = dist.getFid();
			TextDistribution stored = map.get(fid);
			if (stored != null) {
				stored.addPositions(dist.getPositions());
			} else {
				map.put(fid, dist);
			}
		}
	}

}
