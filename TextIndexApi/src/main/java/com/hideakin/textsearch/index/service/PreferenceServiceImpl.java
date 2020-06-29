package com.hideakin.textsearch.index.service;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@Service
@Transactional
public class PreferenceServiceImpl implements PreferenceService {

	private static final Logger logger = LoggerFactory.getLogger(PreferenceServiceImpl.class);

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private PreferenceRepository preferenceRepository;

	@Override
	public String getPreference(String name) {
		PreferenceEntity entity = preferenceRepository.findByName(name);
		return entity != null ? entity.getValue() : null;
	}

	@Override
	public void createPreference(String name, String value) {
		preferenceRepository.save(new PreferenceEntity(name, value));
	}

	@Override
	public boolean updatePreference(String name, String value) {
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity == null) {
			return false;
		}
		entity.setValue(value);
		preferenceRepository.save(entity);
		return true;
	}

	@Override
	public boolean deletePreference(String name) {
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity == null) {
			return false;
		}
		preferenceRepository.delete(entity);
		return true;
	}
	
	@Override
	public boolean isServiceUnavailable() {
		PreferenceEntity entity = preferenceRepository.findByName("enabled");
		return "false".equalsIgnoreCase(entity != null ? entity.getValue() : null);
	}

	@Override
	public void setServiceAvailability(boolean value) {
		preferenceRepository.save(new PreferenceEntity("enabled", value ? "true" : "false"));
		logger.warn("{}", value ? "Exited maintenance mode." : "Entered maintenance mode.");
	}

}
